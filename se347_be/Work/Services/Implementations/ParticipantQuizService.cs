using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.Database.Entities;
using se347_be.Work.DTOs.Participant;
using se347_be.Work.Services.Interfaces;
using se347_be.Email;

namespace se347_be.Work.Services.Implementations
{
    public class ParticipantQuizService : IParticipantQuizService
    {
        private readonly MyAppDbContext _context;
        private readonly IEmail _emailService;

        public ParticipantQuizService(MyAppDbContext context, IEmail emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<QuizInfoDTO> GetQuizInfoAsync(Guid quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizQuestions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                throw new InvalidDataException("Quiz not found");
            }

            var now = DateTime.Now;
            bool isAvailable = true;
            string? message = null;

            if (!quiz.IsPublish)
            {
                isAvailable = false;
                message = "This quiz is not published yet";
            }
            else if (quiz.StartTime.HasValue && now < quiz.StartTime.Value)
            {
                isAvailable = false;
                message = $"Quiz will be available from {quiz.StartTime.Value:dd/MM/yyyy HH:mm}";
            }
            else if (quiz.DueTime.HasValue && now > quiz.DueTime.Value)
            {
                isAvailable = false;
                message = "Quiz has expired";
            }

            return new QuizInfoDTO
            {
                QuizId = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                StartTime = quiz.StartTime,
                DueTime = quiz.DueTime,
                DurationInMinutes = quiz.DurationInMinutes,
                TotalQuestions = quiz.QuizQuestions?.Count ?? 0,
                RequiresAccessCode = !string.IsNullOrEmpty(quiz.AccessCode),
                IsAvailable = isAvailable,
                Message = message,
                PresentationMode = quiz.PresentationMode,
                AllowNavigationBack = quiz.AllowNavigationBack
            };
        }

        public async Task<StartQuizResponseDTO> StartQuizAsync(Guid quizId, StartQuizRequestDTO dto)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.Question)
                        .ThenInclude(q => q!.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                throw new InvalidDataException("Quiz not found");
            }

            if (!quiz.IsPublish)
            {
                throw new InvalidDataException("This quiz is not published yet");
            }

            // Verify access code if required
            if (!string.IsNullOrEmpty(quiz.AccessCode))
            {
                if (string.IsNullOrEmpty(dto.AccessCode) || dto.AccessCode != quiz.AccessCode)
                {
                    throw new UnauthorizedAccessException("Invalid access code");
                }
            }

            // Check if quiz has started and not expired
            var now = DateTime.Now;
            if (quiz.StartTime.HasValue && now < quiz.StartTime.Value)
            {
                throw new InvalidDataException("Quiz has not started yet");
            }

            if (quiz.DueTime.HasValue && now > quiz.DueTime.Value)
            {
                throw new InvalidDataException("Quiz has expired");
            }

            // Check whitelist for Private quizzes
            if (quiz.AccessType == "Private")
            {
                bool isInvited = await _context.QuizInvitations
                    .AnyAsync(i => i.QuizId == quizId && 
                                   (i.Email == dto.Email || 
                                    (!string.IsNullOrEmpty(dto.StudentId) && i.StudentId == dto.StudentId)));

                if (!isInvited)
                {
                    throw new UnauthorizedAccessException("You are not invited to this quiz. This is a private quiz.");
                }
            }

            // Check attempt limit by StudentId (more reliable than Email to prevent cheating)
            if (!string.IsNullOrEmpty(dto.StudentId))
            {
                var attemptCount = await _context.QuizParticipations
                    .Where(p => p.QuizId == quizId && p.StudentId == dto.StudentId)
                    .CountAsync();

                if (attemptCount >= quiz.MaxTimesCanAttempt)
                {
                    throw new InvalidDataException($"Student ID {dto.StudentId} has reached the maximum number of attempts ({quiz.MaxTimesCanAttempt})");
                }
            }
            else
            {
                // Fallback to email if StudentId not provided (for anonymous quizzes)
                var attemptCount = await _context.QuizParticipations
                    .Where(p => p.QuizId == quizId && p.Email == dto.Email)
                    .CountAsync();

                if (attemptCount >= quiz.MaxTimesCanAttempt)
                {
                    throw new InvalidDataException($"You have reached the maximum number of attempts ({quiz.MaxTimesCanAttempt})");
                }
            }

            // Get questions in order
            var questions = quiz.QuizQuestions?
                .OrderBy(qq => qq.OrderIndex)
                .Select(qq => qq.Question!)
                .ToList() ?? new List<Question>();

            // Generate shuffle JSON if needed
            string? shuffledQuestionsJson = null;
            string? shuffledAnswersJson = null;

            if (quiz.IsShuffleQuestions || quiz.IsShuffleAnswers)
            {
                var questionList = questions.ToList();

                // Shuffle questions
                if (quiz.IsShuffleQuestions)
                {
                    questionList = questionList.OrderBy(x => Guid.NewGuid()).ToList();
                    shuffledQuestionsJson = System.Text.Json.JsonSerializer.Serialize(
                        questionList.Select(q => q.Id).ToList());
                }

                // Shuffle answers
                if (quiz.IsShuffleAnswers)
                {
                    var answersMap = new Dictionary<Guid, List<Guid>>();
                    foreach (var question in questions)
                    {
                        var answerIds = question.Answers?
                            .OrderBy(x => Guid.NewGuid())
                            .Select(a => a.Id)
                            .ToList() ?? new List<Guid>();
                        answersMap[question.Id] = answerIds;
                    }
                    shuffledAnswersJson = System.Text.Json.JsonSerializer.Serialize(answersMap);
                }
            }

            // Create participation with shuffle data
            var participation = new QuizParticipation
            {
                Id = Guid.NewGuid(),
                QuizId = quizId,
                FullName = dto.FullName,
                Email = dto.Email,
                StudentId = dto.StudentId,
                ClassName = dto.ClassName,
                ParticipationTime = DateTime.Now,
                SubmitTime = null,
                ShuffledQuestionsJson = shuffledQuestionsJson,
                ShuffledAnswersJson = shuffledAnswersJson
            };

            _context.QuizParticipations.Add(participation);
            await _context.SaveChangesAsync();

            var estimatedEndTime = quiz.DurationInMinutes.HasValue
                ? DateTime.Now.AddMinutes(quiz.DurationInMinutes.Value)
                : (DateTime?)null;

            return new StartQuizResponseDTO
            {
                ParticipationId = participation.Id,
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,
                Description = quiz.Description,
                DurationInMinutes = quiz.DurationInMinutes,
                StartTime = participation.ParticipationTime,
                EstimatedEndTime = estimatedEndTime,
                TotalQuestions = quiz.QuizQuestions?.Count ?? 0,
                IsShuffleQuestions = quiz.IsShuffleQuestions,
                IsShuffleAnswers = quiz.IsShuffleAnswers
            };
        }

        public async Task<QuizContentDTO> GetQuizContentAsync(Guid participationId)
        {
            var participation = await _context.QuizParticipations
                .Include(p => p.Quiz)
                    .ThenInclude(q => q!.QuizQuestions!)
                        .ThenInclude(qq => qq.Question!)
                            .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(p => p.Id == participationId);

            if (participation == null)
            {
                throw new InvalidDataException("Participation not found");
            }

            if (participation.SubmitTime.HasValue)
            {
                throw new InvalidDataException("Quiz has already been submitted");
            }

            var quiz = participation.Quiz!;

            // Get questions from QuizQuestion with proper order
            var quizQuestions = quiz.QuizQuestions?.
                OrderBy(qq => qq.OrderIndex)
                .Select(qq => qq.Question!)
                .ToList() ?? new List<Question>();

            List<Question> orderedQuestions;

            // Use shuffled order from JSON if exists
            if (!string.IsNullOrEmpty(participation.ShuffledQuestionsJson))
            {
                var shuffledIds = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(participation.ShuffledQuestionsJson) ?? new List<Guid>();
                orderedQuestions = shuffledIds
                    .Select(id => quizQuestions.FirstOrDefault(q => q.Id == id))
                    .Where(q => q != null)
                    .Cast<Question>()
                    .ToList();
            }
            else
            {
                // Use original order (by OrderIndex)
                orderedQuestions = quizQuestions;
            }

            // Deserialize shuffled answers map if exists
            Dictionary<Guid, List<Guid>>? shuffledAnswersMap = null;
            if (!string.IsNullOrEmpty(participation.ShuffledAnswersJson))
            {
                shuffledAnswersMap = System.Text.Json.JsonSerializer.Deserialize<Dictionary<Guid, List<Guid>>>(participation.ShuffledAnswersJson);
            }

            var questionDTOs = new List<QuestionContentDTO>();
            int orderNumber = 1;

            foreach (var question in orderedQuestions)
            {
                var answers = question.Answers ?? new List<Answer>();

                // Use shuffled answer order if exists
                if (shuffledAnswersMap != null && shuffledAnswersMap.ContainsKey(question.Id))
                {
                    var shuffledAnswerIds = shuffledAnswersMap[question.Id];
                    var orderedAnswers = new List<Answer>();
                    foreach (var answerId in shuffledAnswerIds)
                    {
                        var answer = answers.FirstOrDefault(a => a.Id == answerId);
                        if (answer != null) orderedAnswers.Add(answer);
                    }
                    answers = orderedAnswers;
                }

                var answerDTOs = answers.Select((a, index) => new AnswerOptionDTO
                {
                    AnswerId = a.Id,
                    Content = a.Content,
                    OptionLabel = ((char)('A' + index)).ToString()
                }).ToList();

                questionDTOs.Add(new QuestionContentDTO
                {
                    QuestionId = question.Id,
                    Content = question.Content,
                    Answers = answerDTOs,
                    OrderNumber = orderNumber++
                });
            }

            return new QuizContentDTO
            {
                Questions = questionDTOs,
                ParticipationId = participationId,
                DurationInMinutes = quiz.DurationInMinutes
            };
        }

        public async Task SaveAnswerAsync(Guid participationId, Guid questionId, Guid answerId)
        {
            var participation = await _context.QuizParticipations
                .FirstOrDefaultAsync(p => p.Id == participationId);

            if (participation == null)
            {
                throw new InvalidDataException("Participation not found");
            }

            if (participation.SubmitTime.HasValue)
            {
                throw new InvalidDataException("Quiz has already been submitted");
            }

            // Check if this exact answer already selected (prevent duplicate)
            var exists = await _context.AnswerSelections
                .AnyAsync(s => s.ParticipationId == participationId && s.AnswerId == answerId);

            if (exists)
            {
                // Already selected - toggle off (remove it)
                var existing = await _context.AnswerSelections
                    .FirstOrDefaultAsync(s => s.ParticipationId == participationId && s.AnswerId == answerId);
                if (existing != null)
                {
                    _context.AnswerSelections.Remove(existing);
                }
            }
            else
            {
                // Add new selection (support multi-choice - don't remove other answers for same question)
                var answerSelection = new AnswerSelection
                {
                    Id = Guid.NewGuid(),
                    ParticipationId = participationId,
                    AnswerId = answerId
                };
                _context.AnswerSelections.Add(answerSelection);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<SubmitQuizResponseDTO> SubmitQuizAsync(Guid participationId, SubmitQuizDTO dto)
        {
            var participation = await _context.QuizParticipations
                .Include(p => p.Quiz!)
                    .ThenInclude(q => q.QuizQuestions!)
                        .ThenInclude(qq => qq.Question!)
                            .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(p => p.Id == participationId);

            if (participation == null)
            {
                throw new InvalidDataException("Participation not found");
            }

            if (participation.SubmitTime.HasValue)
            {
                throw new InvalidDataException("Quiz has already been submitted");
            }

            var quiz = participation.Quiz!;

            // Calculate score from already saved answers (Source of Truth from save-answer API)
            // No need to process dto.Answers - participant already saved via save-answer/save-answers
            var selectedAnswers = await _context.AnswerSelections
                .Where(s => s.ParticipationId == participationId)
                .Include(s => s.Answer)
                .ThenInclude(a => a!.Question)
                .ToListAsync();

            int correctCount = selectedAnswers.Count(s => s.Answer!.IsCorrectAnswer);
            int totalQuestions = quiz.QuizQuestions?.Count ?? 0;

            // Calculate total possible points (sum of all question points)
            int totalPoints = quiz.QuizQuestions?.Sum(qq => qq.Question!.Points) ?? totalQuestions;
            
            // Calculate earned points (sum of points from correct answers)
            int earnedPoints = 0;
            foreach (var selection in selectedAnswers)
            {
                if (selection.Answer!.IsCorrectAnswer)
                {
                    earnedPoints += selection.Answer.Question?.Points ?? 1;
                }
            }

            // Scale score to 10 points (not 100)
            decimal score = totalPoints > 0 ? (decimal)earnedPoints / totalPoints * 10 : 0;

            // Update participation
            participation.SubmitTime = DateTime.Now;
            participation.Score = Math.Round(score, 2);
            
            // Clear shuffle JSON to save storage (no longer needed after submit)
            participation.ShuffledQuestionsJson = null;
            participation.ShuffledAnswersJson = null;
            
            await _context.SaveChangesAsync();

            // Send email if enabled
            if (quiz.SendResultEmail && !string.IsNullOrEmpty(participation.Email))
            {
                try
                {
                    await _emailService.SendQuizResultEmailAsync(
                        participation.Email,
                        participation.FullName ?? "Participant",
                        quiz.Title,
                        participation.Score.Value,
                        correctCount,
                        totalQuestions
                    );
                }
                catch (Exception ex)
                {
                    // Log but don't fail the submission
                    Console.WriteLine($"Failed to send result email: {ex.Message}");
                }
            }

            return new SubmitQuizResponseDTO
            {
                ParticipationId = participationId,
                SubmitTime = participation.SubmitTime.Value,
                Score = quiz.ShowScoreAfterSubmission ? participation.Score : null,
                TotalQuestions = totalQuestions,
                CorrectAnswers = quiz.ShowScoreAfterSubmission ? correctCount : 0,
                ShowScore = quiz.ShowScoreAfterSubmission,
                Message = quiz.ShowScoreAfterSubmission 
                    ? $"You scored {participation.Score:F2}%!" 
                    : "Quiz submitted successfully! Results will be sent to your email."
            };
        }

        public async Task SaveAnswersAsync(Guid participationId, List<AnswerSubmissionDTO> answers)
        {
            var participation = await _context.QuizParticipations
                .FirstOrDefaultAsync(p => p.Id == participationId);

            if (participation == null)
            {
                throw new InvalidDataException("Participation not found");
            }

            if (participation.SubmitTime.HasValue)
            {
                throw new InvalidDataException("Quiz has already been submitted");
            }

            // Save each answer
            foreach (var answer in answers)
            {
                await SaveAnswerAsync(participationId, answer.QuestionId, answer.SelectedAnswerId);
            }
        }

        public async Task<SubmitQuizResponseDTO> GetParticipationResultAsync(Guid participationId)
        {
            var participation = await _context.QuizParticipations
                .Include(p => p.Quiz)
                .FirstOrDefaultAsync(p => p.Id == participationId);

            if (participation == null)
            {
                throw new InvalidDataException("Participation not found");
            }

            if (!participation.SubmitTime.HasValue)
            {
                throw new InvalidDataException("Quiz has not been submitted yet");
            }

            var quiz = participation.Quiz!;

            // Determine if score should be shown
            bool showScore = quiz.ShowScoreAfterSubmission;

            // Determine if correct answers should be shown based on ShowCorrectAnswersMode
            bool showCorrectAnswers = false;
            var now = DateTime.Now;

            switch (quiz.ShowCorrectAnswersMode)
            {
                case "Immediately":
                    showCorrectAnswers = true;
                    break;
                case "AfterDueTime":
                    if (quiz.DueTime.HasValue && now > quiz.DueTime.Value)
                    {
                        showCorrectAnswers = true;
                    }
                    break;
                case "Never":
                default:
                    showCorrectAnswers = false;
                    break;
            }

            // Get answer selections to count correct answers
            var selectedAnswers = await _context.AnswerSelections
                .Where(s => s.ParticipationId == participationId)
                .Include(s => s.Answer)
                .ToListAsync();

            int correctCount = selectedAnswers.Count(s => s.Answer!.IsCorrectAnswer);
            int totalQuestions = quiz.QuizQuestions?.Count ?? 0;

            // Build message based on what user can see
            string message;
            if (showScore && showCorrectAnswers)
            {
                message = $"You scored {participation.Score:F2} points! Correct answers: {correctCount}/{totalQuestions}";
            }
            else if (showScore)
            {
                message = $"You scored {participation.Score:F2} points!";
            }
            else if (showCorrectAnswers)
            {
                message = $"Quiz submitted. You got {correctCount}/{totalQuestions} correct.";
            }
            else
            {
                message = "Quiz submitted. Results will be available later.";
            }

            return new SubmitQuizResponseDTO
            {
                ParticipationId = participationId,
                SubmitTime = participation.SubmitTime.Value,
                Score = showScore ? participation.Score : null,
                TotalQuestions = totalQuestions,
                CorrectAnswers = showCorrectAnswers ? correctCount : 0,
                ShowScore = showScore,
                Message = message
            };
        }
    }
}
