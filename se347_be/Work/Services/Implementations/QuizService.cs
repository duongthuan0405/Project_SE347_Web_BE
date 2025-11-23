using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.Database.Entities;
using se347_be.Work.DTOs.Answer;
using se347_be.Work.DTOs.Question;
using se347_be.Work.DTOs.Quiz;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;
using se347_be.Work.Storage.Interfaces;
using se347_be.Work.DTOs.QuestionBank;

namespace se347_be.Work.Services.Implementations
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuestionBankRepository _questionBankRepository;
        private readonly MyAppDbContext _context;
        private readonly IGeminiAIService _geminiService;
        private readonly IDocumentProcessorService _docProcessor;
        private readonly IAnswerRepository _answerRepository;
 

        public QuizService(
            IQuizRepository quizRepository,
            IQuestionBankRepository questionBankRepository,
            MyAppDbContext context,
            IGeminiAIService geminiService,
            IDocumentProcessorService docProcessor,
            IQuestionRepository questionRepository,
            IAnswerRepository answerRepository)
        {
            _quizRepository = quizRepository;
            _questionBankRepository = questionBankRepository;
            _context = context;
            _geminiService = geminiService;
            _docProcessor = docProcessor;
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
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
                ScoringMode = createQuizDTO.ScoringMode,
                CreatorId = creatorId,
                CreateAt = DateTime.UtcNow
            };

            await _quizRepository.CreateQuizAsync(quiz);

            return new QuizResponseDTO
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                CreateAt = DateTime.SpecifyKind(quiz.CreateAt, DateTimeKind.Utc),
                StartTime = DateTime.SpecifyKind(quiz.StartTime ?? DateTime.UtcNow, DateTimeKind.Utc),
                DueTime = DateTime.SpecifyKind(quiz.DueTime ?? DateTime.UtcNow, DateTimeKind.Utc),
                MaxTimesCanAttempt = quiz.MaxTimesCanAttempt,
                IsPublish = quiz.IsPublish,
                IsShuffleAnswers = quiz.IsShuffleAnswers,
                IsShuffleQuestions = quiz.IsShuffleQuestions,
                DurationInMinutes = quiz.DurationInMinutes,
                AccessCode = quiz.AccessCode,
                AccessType = quiz.AccessType,
                ShowScoreAfterSubmission = quiz.ShowScoreAfterSubmission,
                SendResultEmail = quiz.SendResultEmail,
                ShowCorrectAnswersMode = quiz.ShowCorrectAnswersMode,
                AllowNavigationBack = quiz.AllowNavigationBack,
                PresentationMode = quiz.PresentationMode,
                ScoringMode = quiz.ScoringMode,
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
                CreateAt = DateTime.SpecifyKind(q.CreateAt, DateTimeKind.Utc),
                StartTime = DateTime.SpecifyKind(q.StartTime ?? DateTime.UtcNow, DateTimeKind.Utc),
                DueTime = DateTime.SpecifyKind(q.DueTime ?? DateTime.UtcNow, DateTimeKind.Utc),
                MaxTimesCanAttempt = q.MaxTimesCanAttempt,
                IsPublish = q.IsPublish,
                IsShuffleAnswers = q.IsShuffleAnswers,
                IsShuffleQuestions = q.IsShuffleQuestions,
                DurationInMinutes = q.DurationInMinutes,
                AccessCode = q.AccessCode,
                AccessType = q.AccessType,
                ShowScoreAfterSubmission = q.ShowScoreAfterSubmission,
                SendResultEmail = q.SendResultEmail,
                ShowCorrectAnswersMode = q.ShowCorrectAnswersMode,
                AllowNavigationBack = q.AllowNavigationBack,
                PresentationMode = q.PresentationMode,
                ScoringMode = q.ScoringMode,
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
                CreateAt = DateTime.SpecifyKind(quiz.CreateAt, DateTimeKind.Utc),
                StartTime = DateTime.SpecifyKind(quiz.StartTime ?? DateTime.UtcNow, DateTimeKind.Utc),
                DueTime = DateTime.SpecifyKind(quiz.DueTime ?? DateTime.UtcNow, DateTimeKind.Utc),
                MaxTimesCanAttempt = quiz.MaxTimesCanAttempt,
                IsPublish = quiz.IsPublish,
                IsShuffleAnswers = quiz.IsShuffleAnswers,
                IsShuffleQuestions = quiz.IsShuffleQuestions,
                DurationInMinutes = quiz.DurationInMinutes,
                AccessCode = quiz.AccessCode,
                AccessType = quiz.AccessType,
                ShowScoreAfterSubmission = quiz.ShowScoreAfterSubmission,
                SendResultEmail = quiz.SendResultEmail,
                ShowCorrectAnswersMode = quiz.ShowCorrectAnswersMode,
                AllowNavigationBack = quiz.AllowNavigationBack,
                PresentationMode = quiz.PresentationMode,
                ScoringMode = quiz.ScoringMode,
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

            // Check if publishing for the first time
            bool wasUnpublished = !quiz.IsPublish;
            
            if (updateQuizDTO.IsPublish.HasValue)
                quiz.IsPublish = updateQuizDTO.IsPublish.Value;

            // ⭐ Auto-save AI questions to bank when publishing
            if (quiz.IsPublish && wasUnpublished && !quiz.QuestionsSavedToBank)
            {
                await SaveQuizQuestionsToBankAsync(quizId, creatorId);
            }

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

            if (updateQuizDTO.ScoringMode != null)
                quiz.ScoringMode = updateQuizDTO.ScoringMode;

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
                CreateAt = DateTime.SpecifyKind(quiz.CreateAt, DateTimeKind.Utc),
                StartTime = DateTime.SpecifyKind(quiz.StartTime ?? DateTime.UtcNow, DateTimeKind.Utc),
                DueTime = DateTime.SpecifyKind(quiz.DueTime ?? DateTime.UtcNow, DateTimeKind.Utc),
                MaxTimesCanAttempt = quiz.MaxTimesCanAttempt,
                IsPublish = quiz.IsPublish,
                IsShuffleAnswers = quiz.IsShuffleAnswers,
                IsShuffleQuestions = quiz.IsShuffleQuestions,
                DurationInMinutes = quiz.DurationInMinutes,
                AccessCode = quiz.AccessCode,
                AccessType = quiz.AccessType,
                ShowScoreAfterSubmission = quiz.ShowScoreAfterSubmission,
                SendResultEmail = quiz.SendResultEmail,
                ShowCorrectAnswersMode = quiz.ShowCorrectAnswersMode,
                AllowNavigationBack = quiz.AllowNavigationBack,
                PresentationMode = quiz.PresentationMode,
                ScoringMode = quiz.ScoringMode,
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

        public async Task CreateQuestionInQuizAsync(Guid quizId, CreateQuestionInQuizDTO dto, Guid creatorId)
        {
            // Verify quiz ownership
            var isOwned = await _quizRepository.IsQuizOwnedByUserAsync(quizId, creatorId);
            if (!isOwned)
            {
                throw new UnauthorizedAccessException("You don't have permission to modify this quiz");
            }

            var existingQuestionIds = await _context.QuizQuestions
                .Where(qq => qq.QuizId == quizId)
                .Select(qq => qq.QuestionId)
                .ToHashSetAsync();


            if (dto.Id == null || !existingQuestionIds.Contains(Guid.Parse(dto.Id)))
            {
                // Validate at least one correct answer
                if (!dto.Answers.Any(a => a.IsCorrectAnswer))
                {
                    throw new InvalidDataException("At least one answer must be marked as correct");
                }

                var question = new Question
                {

                    Content = dto.Content,
                    Points = dto.Points,
        
                    IsDraft = dto.IsDraft,
                    CreatorId = creatorId
                };

                Guid newQuestionId = await _questionRepository.CreateQuestionAsync(question);

                var newAnswers = dto.Answers.Select(a => new Answer()
                {
                    QuestionId = newQuestionId,
                    Content = a.Content,
                    IsCorrectAnswer = a.IsCorrectAnswer
                }).ToList();

                await _answerRepository.CreateAnswersAsync(newAnswers);



                // Get max OrderIndex for auto-increment
                var maxOrder = await _context.QuizQuestions
                    .Where(qq => qq.QuizId == quizId)
                    .MaxAsync(qq => (int?)qq.OrderIndex) ?? 0;



                // Link question to quiz
                var quizQuestion = new QuizQuestion
                {
                    QuizId = quizId,
                    QuestionId = newQuestionId,
                    OrderIndex = maxOrder + 1
                };

                _context.QuizQuestions.Add(quizQuestion);
                await _context.SaveChangesAsync();

            }
            else
            {
                // Question exists => update
                await _questionRepository.UpdateQuestionAsync(new Question()
                {
                    Id = Guid.Parse(dto.Id),
                    Content = dto.Content,
                    Points = dto.Points,
       
                    IsDraft = dto.IsDraft,
                    CreatorId = creatorId
                });

               

                
                var updatedAnswers = dto.Answers.Select(a => new Answer()
                {
                    QuestionId = Guid.Parse(dto.Id),
                    Content = a.Content,
                    IsCorrectAnswer = a.IsCorrectAnswer
                }).ToList();
                await _answerRepository.UpdateAnswer(updatedAnswers);
                
            }

           
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

        public async Task<QuizDetailDTO> GenerateQuestionsFromDocumentAsync(Guid quizId, GenerateQuestionsFromDocumentDTO dto, Guid creatorId)
        {
            // Get existing quiz
            var quiz = await _context.Quizzes
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.Question)
                        .ThenInclude(q => q!.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                throw new InvalidDataException("Quiz not found");
            }

            if (quiz.CreatorId != creatorId)
            {
                throw new UnauthorizedAccessException("You don't have permission to modify this quiz");
            }

            // Get document (must belong to this quiz)
            var document = await _context.QuizSourceDocuments
                .FirstOrDefaultAsync(d => d.Id == dto.DocumentId && d.QuizId == quizId);

            if (document == null)
            {
                throw new InvalidDataException("Document not found or does not belong to this quiz");
            }

            // Read document content
            string textContent;
            try
            {
                textContent = await ReadDocumentContentAsync(document.StorageUrl);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to read document content", ex);
            }

            // Generate questions from AI with category = "AI"
            var aiResponse = await _geminiService.GenerateQuestionsFromTextAsync(
                textContent,
                document.FileName,
                dto.NumberOfQuestions,
                dto.AdditionalInstructions
            );

            // Get current max order index
            int orderIndex = quiz.QuizQuestions?.Any() == true 
                ? quiz.QuizQuestions.Max(qq => qq.OrderIndex) + 1 
                : 0;

            // Create Questions and link to existing Quiz
            var questionsList = new List<Question>();

            foreach (var questionDto in aiResponse.Questions)
            {
                var question = new Question
                {
                    Id = Guid.NewGuid(),
                    Content = questionDto.Question,
                    Points = 1, // Default points
                    CreatorId = creatorId,
                    Category = "AI", // ⭐ Mark as AI-generated
                    IsDraft = false
                };

                // Add answers
                var answers = questionDto.Answers.Select(a => new Answer
                {
                    Id = Guid.NewGuid(),
                    Content = a.Content,
                    QuestionId = question.Id,
                    IsCorrectAnswer = a.IsCorrect
                }).ToList();

                question.Answers = answers;
                questionsList.Add(question);

                // Link question to quiz
                var quizQuestion = new QuizQuestion
                {
                    QuizId = quiz.Id,
                    QuestionId = question.Id,
                    OrderIndex = orderIndex++
                };

                _context.Questions.Add(question);
                _context.QuizQuestions.Add(quizQuestion);
            }

            await _context.SaveChangesAsync();

            // Reload quiz with all questions
            quiz = await _context.Quizzes
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.Question)
                        .ThenInclude(q => q!.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            // Return Quiz with ALL Questions (including newly generated)
            return new QuizDetailDTO
            {
                Id = quiz!.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                CreateAt = DateTime.SpecifyKind(quiz.CreateAt, DateTimeKind.Utc),
                StartTime = DateTime.SpecifyKind(quiz.StartTime ?? DateTime.UtcNow, DateTimeKind.Utc),
                DueTime = DateTime.SpecifyKind(quiz.DueTime ?? DateTime.UtcNow, DateTimeKind.Utc),
                MaxTimesCanAttempt = quiz.MaxTimesCanAttempt,
                IsPublish = quiz.IsPublish,
                IsShuffleAnswers = quiz.IsShuffleAnswers,
                IsShuffleQuestions = quiz.IsShuffleQuestions,
                DurationInMinutes = quiz.DurationInMinutes,
                AccessCode = quiz.AccessCode,
                AccessType = quiz.AccessType,
                ShowScoreAfterSubmission = quiz.ShowScoreAfterSubmission,
                SendResultEmail = quiz.SendResultEmail,
                ShowCorrectAnswersMode = quiz.ShowCorrectAnswersMode,
                AllowNavigationBack = quiz.AllowNavigationBack,
                PresentationMode = quiz.PresentationMode,
                ScoringMode = quiz.ScoringMode,
                CreatorId = quiz.CreatorId,
                Questions = quiz.QuizQuestions?
                    .OrderBy(qq => qq.OrderIndex)
                    .Select(qq => new QuestionResponseDTO
                    {
                        Id = qq.Question!.Id,
                        Content = qq.Question.Content,
                        QuizId = quiz.Id,
                        Answers = qq.Question.Answers?.Select(a => new AnswerResponseDTO
                        {
                            Id = a.Id,
                            Content = a.Content,
                            IsCorrectAnswer = a.IsCorrectAnswer
                        }).ToList()
                    }).ToList() ?? new List<QuestionResponseDTO>()
            };
        }

        public async Task SaveQuizQuestionsToBankAsync(Guid quizId, Guid creatorId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.Question)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                throw new InvalidDataException("Quiz not found");
            }

            if (quiz.CreatorId != creatorId)
            {
                throw new UnauthorizedAccessException("You don't have permission to access this quiz");
            }

            if (quiz.QuestionsSavedToBank)
            {
                return; // Already saved, skip
            }

            // Questions already exist in database, just mark as saved
            quiz.QuestionsSavedToBank = true;
            await _context.SaveChangesAsync();
        }

        private async Task<string> ReadDocumentContentAsync(string storageUrl)
        {
            if (!File.Exists(storageUrl))
            {
                throw new FileNotFoundException("Document file not found", storageUrl);
            }

            // Get file extension to determine how to read
            var extension = Path.GetExtension(storageUrl).ToLowerInvariant();

            // For PDF and DOCX files, use DocumentProcessor to extract text
            if (extension == ".pdf" || extension == ".docx")
            {
                // Read file into memory stream
                using var fileStream = new FileStream(storageUrl, FileMode.Open, FileAccess.Read);
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Create IFormFile wrapper
                var formFile = new FormFile(memoryStream, 0, memoryStream.Length, "file", Path.GetFileName(storageUrl))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = extension == ".pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                };

                // Extract text using document processor
                return await _docProcessor.ExtractTextFromFileAsync(formFile);
            }

            // For text files, read directly
            if (extension == ".txt")
            {
                return await File.ReadAllTextAsync(storageUrl);
            }

            throw new NotSupportedException($"File type '{extension}' is not supported. Supported types: .txt, .pdf, .docx");
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
