using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.Database.Entities;
using se347_be.Work.DTOs.Answer;
using se347_be.Work.DTOs.Question;
using se347_be.Work.DTOs.Quiz;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace se347_be.Work.Services.Implementations
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IQuestionBankRepository _questionBankRepository;
        private readonly MyAppDbContext _context;

        public QuizService(
            IQuizRepository quizRepository,
            IQuestionBankRepository questionBankRepository,
            MyAppDbContext context)
        {
            _quizRepository = quizRepository;
            _questionBankRepository = questionBankRepository;
            _context = context;
        }

        public async Task<QuizResponseDTO> CreateQuizAsync(CreateQuizDTO createQuizDTO, Guid creatorId)
        {
            // Validate time range
            if (createQuizDTO.StartTime.HasValue && createQuizDTO.DueTime.HasValue)
            {
                if (createQuizDTO.StartTime.Value >= createQuizDTO.DueTime.Value)
                {
                    throw new InvalidDataException("Start time must be before due time");
                }
            }

            // Validate duration
            if (createQuizDTO.DurationInMinutes.HasValue && createQuizDTO.DurationInMinutes.Value <= 0)
            {
                throw new InvalidDataException("Duration must be greater than 0");
            }

            // Validate ShowCorrectAnswersMode with DueTime
            if (createQuizDTO.ShowCorrectAnswersMode == "AfterDueTime" && !createQuizDTO.DueTime.HasValue)
            {
                throw new InvalidDataException("DueTime is required when ShowCorrectAnswersMode is 'AfterDueTime'");
            }

            var quiz = new Quiz
            {
                Id = Guid.NewGuid(),
                Title = createQuizDTO.Title,
                Description = createQuizDTO.Description,
                StartTime = createQuizDTO.StartTime,
                DueTime = createQuizDTO.DueTime,
                MaxTimesCanAttempt = createQuizDTO.MaxTimesCanAttempt,
                IsPublish = createQuizDTO.IsPublish,
                IsShuffleAnswers = createQuizDTO.IsShuffleAnswers,
                IsShuffleQuestions = createQuizDTO.IsShuffleQuestions,
                DurationInMinutes = createQuizDTO.DurationInMinutes,
                AccessCode = GenerateAccessCode(), // Auto-generate 6 character code
                ShowScoreAfterSubmission = createQuizDTO.ShowScoreAfterSubmission,
                SendResultEmail = createQuizDTO.SendResultEmail,
                ShowCorrectAnswersMode = createQuizDTO.ShowCorrectAnswersMode,
                AllowNavigationBack = createQuizDTO.AllowNavigationBack,
                PresentationMode = createQuizDTO.PresentationMode,
                CreatorId = creatorId,
                CreateAt = DateTime.Now
            };

            await _quizRepository.CreateQuizAsync(quiz);

            return new QuizResponseDTO
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                CreateAt = quiz.CreateAt,
                StartTime = quiz.StartTime,
                DueTime = quiz.DueTime,
                MaxTimesCanAttempt = quiz.MaxTimesCanAttempt,
                IsPublish = quiz.IsPublish,
                IsShuffleAnswers = quiz.IsShuffleAnswers,
                IsShuffleQuestions = quiz.IsShuffleQuestions,
                DurationInMinutes = quiz.DurationInMinutes,
                AccessCode = quiz.AccessCode,
                AccessType = quiz.AccessType,
                ShowScoreAfterSubmission = quiz.ShowScoreAfterSubmission,
                SendResultEmail = quiz.SendResultEmail,
                CreatorId = quiz.CreatorId,
                TotalQuestions = 0,
                TotalParticipants = 0
            };
        }

        public async Task<List<QuizResponseDTO>> GetQuizzesByCreatorAsync(Guid creatorId)
        {
            var quizzes = await _quizRepository.GetQuizzesByCreatorIdAsync(creatorId);

            return quizzes.Select(q => new QuizResponseDTO
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                CreateAt = q.CreateAt,
                StartTime = q.StartTime,
                DueTime = q.DueTime,
                MaxTimesCanAttempt = q.MaxTimesCanAttempt,
                IsPublish = q.IsPublish,
                IsShuffleAnswers = q.IsShuffleAnswers,
                IsShuffleQuestions = q.IsShuffleQuestions,
                DurationInMinutes = q.DurationInMinutes,
                AccessCode = q.AccessCode,
                AccessType = q.AccessType,
                ShowScoreAfterSubmission = q.ShowScoreAfterSubmission,
                SendResultEmail = q.SendResultEmail,
                CreatorId = q.CreatorId,
                TotalQuestions = q.QuizQuestions?.Count ?? 0,
                TotalParticipants = q.Participations?.Count ?? 0
            }).ToList();
        }

        public async Task<QuizDetailDTO> GetQuizDetailAsync(Guid quizId, Guid creatorId)
        {
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);

            if (quiz == null)
            {
                throw new InvalidDataException("Quiz not found");
            }

            if (quiz.CreatorId != creatorId)
            {
                throw new UnauthorizedAccessException("You don't have permission to access this quiz");
            }

            return new QuizDetailDTO
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                CreateAt = quiz.CreateAt,
                StartTime = quiz.StartTime,
                DueTime = quiz.DueTime,
                MaxTimesCanAttempt = quiz.MaxTimesCanAttempt,
                IsPublish = quiz.IsPublish,
                IsShuffleAnswers = quiz.IsShuffleAnswers,
                IsShuffleQuestions = quiz.IsShuffleQuestions,
                DurationInMinutes = quiz.DurationInMinutes,
                AccessCode = quiz.AccessCode,
                ShowScoreAfterSubmission = quiz.ShowScoreAfterSubmission,
                SendResultEmail = quiz.SendResultEmail,
                CreatorId = quiz.CreatorId,
                Questions = quiz.QuizQuestions?
                    .OrderBy(qq => qq.OrderIndex)
                    .Select(qq => new QuestionResponseDTO
                    {
                        Id = qq.Question!.Id,
                        Content = qq.Question.Content,
                        QuizId = quiz.Id, // Use quiz.Id instead of question.QuizId
                        Answers = qq.Question.Answers?.Select(a => new AnswerResponseDTO
                        {
                            Id = a.Id,
                            Content = a.Content,
                            IsCorrectAnswer = a.IsCorrectAnswer
                        }).ToList()
                    }).ToList()
            };
        }

        public async Task<QuizResponseDTO> UpdateQuizAsync(Guid quizId, UpdateQuizDTO updateQuizDTO, Guid creatorId)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);

            if (quiz == null)
            {
                throw new InvalidDataException("Quiz not found");
            }

            if (quiz.CreatorId != creatorId)
            {
                throw new UnauthorizedAccessException("You don't have permission to update this quiz");
            }

            // Validate duration if provided
            if (updateQuizDTO.DurationInMinutes.HasValue && updateQuizDTO.DurationInMinutes.Value <= 0)
            {
                throw new InvalidDataException("Duration must be greater than 0");
            }

            // Update only fields that are provided
            if (updateQuizDTO.Title != null)
                quiz.Title = updateQuizDTO.Title;

            if (updateQuizDTO.Description != null)
                quiz.Description = updateQuizDTO.Description;

            if (updateQuizDTO.StartTime.HasValue)
                quiz.StartTime = updateQuizDTO.StartTime;

            if (updateQuizDTO.DueTime.HasValue)
                quiz.DueTime = updateQuizDTO.DueTime;

            if (updateQuizDTO.MaxTimesCanAttempt.HasValue)
                quiz.MaxTimesCanAttempt = updateQuizDTO.MaxTimesCanAttempt.Value;

            if (updateQuizDTO.IsPublish.HasValue)
                quiz.IsPublish = updateQuizDTO.IsPublish.Value;

            if (updateQuizDTO.IsShuffleAnswers.HasValue)
                quiz.IsShuffleAnswers = updateQuizDTO.IsShuffleAnswers.Value;

            if (updateQuizDTO.IsShuffleQuestions.HasValue)
                quiz.IsShuffleQuestions = updateQuizDTO.IsShuffleQuestions.Value;

            if (updateQuizDTO.DurationInMinutes.HasValue)
                quiz.DurationInMinutes = updateQuizDTO.DurationInMinutes;

            if (updateQuizDTO.AccessCode != null)
                quiz.AccessCode = updateQuizDTO.AccessCode;

            if (updateQuizDTO.ShowScoreAfterSubmission.HasValue)
                quiz.ShowScoreAfterSubmission = updateQuizDTO.ShowScoreAfterSubmission.Value;

            if (updateQuizDTO.SendResultEmail.HasValue)
                quiz.SendResultEmail = updateQuizDTO.SendResultEmail.Value;

            if (updateQuizDTO.ShowCorrectAnswersMode != null)
                quiz.ShowCorrectAnswersMode = updateQuizDTO.ShowCorrectAnswersMode;

            if (updateQuizDTO.AllowNavigationBack.HasValue)
                quiz.AllowNavigationBack = updateQuizDTO.AllowNavigationBack.Value;

            if (updateQuizDTO.PresentationMode != null)
                quiz.PresentationMode = updateQuizDTO.PresentationMode;

            // Validate time range after all updates
            if (quiz.StartTime.HasValue && quiz.DueTime.HasValue)
            {
                if (quiz.StartTime.Value >= quiz.DueTime.Value)
                {
                    throw new InvalidDataException("Start time must be before due time");
                }
            }

            // Validate ShowCorrectAnswersMode with DueTime
            if (quiz.ShowCorrectAnswersMode == "AfterDueTime" && !quiz.DueTime.HasValue)
            {
                throw new InvalidDataException("DueTime is required when ShowCorrectAnswersMode is 'AfterDueTime'");
            }

            await _quizRepository.UpdateQuizAsync(quiz);

            return new QuizResponseDTO
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                CreateAt = quiz.CreateAt,
                StartTime = quiz.StartTime,
                DueTime = quiz.DueTime,
                MaxTimesCanAttempt = quiz.MaxTimesCanAttempt,
                IsPublish = quiz.IsPublish,
                IsShuffleAnswers = quiz.IsShuffleAnswers,
                IsShuffleQuestions = quiz.IsShuffleQuestions,
                DurationInMinutes = quiz.DurationInMinutes,
                AccessCode = quiz.AccessCode,
                AccessType = quiz.AccessType,
                ShowScoreAfterSubmission = quiz.ShowScoreAfterSubmission,
                SendResultEmail = quiz.SendResultEmail,
                CreatorId = quiz.CreatorId,
                TotalQuestions = 0,
                TotalParticipants = quiz.Participations?.Count ?? 0
            };
        }

        public async Task DeleteQuizAsync(Guid quizId, Guid creatorId)
        {
            var isOwned = await _quizRepository.IsQuizOwnedByUserAsync(quizId, creatorId);

            if (!isOwned)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this quiz");
            }

            await _quizRepository.DeleteQuizAsync(quizId);
        }

        public async Task<DTOs.QuestionBank.QuestionBankDetailDTO> CreateQuestionInQuizAsync(Guid quizId, CreateQuestionInQuizDTO dto, Guid creatorId)
        {
            // Verify quiz ownership
            var isOwned = await _quizRepository.IsQuizOwnedByUserAsync(quizId, creatorId);
            if (!isOwned)
            {
                throw new UnauthorizedAccessException("You don't have permission to modify this quiz");
            }

            // Validate at least one correct answer
            if (!dto.Answers.Any(a => a.IsCorrectAnswer))
            {
                throw new InvalidDataException("At least one answer must be marked as correct");
            }

            // Create question
            var question = new Question
            {
                Id = Guid.NewGuid(),
                Content = dto.Content,
                Points = dto.Points,
                Category = dto.Category,
                IsDraft = dto.IsDraft,
                CreatorId = creatorId
            };

            _context.Questions.Add(question);

            // Create answers
            var answers = dto.Answers.Select(a => new Answer
            {
                Id = Guid.NewGuid(),
                Content = a.Content,
                IsCorrectAnswer = a.IsCorrectAnswer,
                QuestionId = question.Id
            }).ToList();

            _context.Answers.AddRange(answers);

            // Get max OrderIndex for auto-increment
            var maxOrder = await _context.QuizQuestions
                .Where(qq => qq.QuizId == quizId)
                .MaxAsync(qq => (int?)qq.OrderIndex) ?? 0;

            // Link question to quiz
            var quizQuestion = new QuizQuestion
            {
                QuizId = quizId,
                QuestionId = question.Id,
                OrderIndex = maxOrder + 1
            };

            _context.QuizQuestions.Add(quizQuestion);
            await _context.SaveChangesAsync();

            // Return response DTO
            return new DTOs.QuestionBank.QuestionBankDetailDTO
            {
                Id = question.Id,
                Content = question.Content,
                Points = question.Points,
                Category = question.Category,
                IsDraft = question.IsDraft,
                UsedInQuizCount = 1,
                Answers = answers.Select(a => new DTOs.QuestionBank.AnswerDetailDTO
                {
                    Id = a.Id,
                    Content = a.Content,
                    IsCorrectAnswer = a.IsCorrectAnswer
                }).ToList()
            };
        }

        public async Task AddQuestionToQuizAsync(Guid quizId, Guid questionId, Guid creatorId)
        {
            // Verify quiz ownership
            var isOwned = await _quizRepository.IsQuizOwnedByUserAsync(quizId, creatorId);
            if (!isOwned)
            {
                throw new UnauthorizedAccessException("You don't have permission to modify this quiz");
            }

            // Verify question ownership
            var isQuestionOwner = await _questionBankRepository.IsOwnerAsync(questionId, creatorId);
            if (!isQuestionOwner)
            {
                throw new UnauthorizedAccessException("You don't have permission to use this question");
            }

            // Check if question already in quiz
            var exists = await _context.QuizQuestions
                .AnyAsync(qq => qq.QuizId == quizId && qq.QuestionId == questionId);

            if (exists)
            {
                throw new InvalidOperationException("Question is already added to this quiz");
            }

            // Get max OrderIndex for auto-increment
            var maxOrder = await _context.QuizQuestions
                .Where(qq => qq.QuizId == quizId)
                .MaxAsync(qq => (int?)qq.OrderIndex) ?? 0;

            var quizQuestion = new QuizQuestion
            {
                QuizId = quizId,
                QuestionId = questionId,
                OrderIndex = maxOrder + 1
            };

            _context.QuizQuestions.Add(quizQuestion);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveQuestionFromQuizAsync(Guid quizId, Guid questionId, Guid creatorId)
        {
            // Verify quiz ownership
            var isOwned = await _quizRepository.IsQuizOwnedByUserAsync(quizId, creatorId);
            if (!isOwned)
            {
                throw new UnauthorizedAccessException("You don't have permission to modify this quiz");
            }

            var quizQuestion = await _context.QuizQuestions
                .FirstOrDefaultAsync(qq => qq.QuizId == quizId && qq.QuestionId == questionId);

            if (quizQuestion == null)
            {
                throw new InvalidDataException("Question not found in this quiz");
            }

            _context.QuizQuestions.Remove(quizQuestion);
            await _context.SaveChangesAsync();
        }

        private string GenerateAccessCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude ambiguous: I,O,0,1
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
