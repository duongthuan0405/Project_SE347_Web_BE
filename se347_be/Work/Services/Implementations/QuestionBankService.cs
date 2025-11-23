using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs.QuestionBank;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class QuestionBankService : IQuestionBankService
    {
        private readonly IQuestionBankRepository _questionRepo;
        private readonly MyAppDbContext _context;

        public QuestionBankService(IQuestionBankRepository questionRepo, MyAppDbContext context)
        {
            _questionRepo = questionRepo;
            _context = context;
        }

        public async Task<QuestionBankDetailDTO> CreateQuestionAsync(CreateQuestionBankDTO dto, Guid creatorId)
        {
            // Validate at least one correct answer
            if (!dto.Answers.Any(a => a.IsCorrectAnswer))
            {
                throw new InvalidDataException("Question must have at least one correct answer");
            }

            var question = new Question
            {
                Id = Guid.NewGuid(),
                Content = dto.Content,
                Points = dto.Points,
                Category = dto.Category,
                CreatorId = creatorId,
                IsDraft = false
            };

            // Create answers
            var answers = dto.Answers.Select(a => new Answer
            {
                Id = Guid.NewGuid(),
                Content = a.Content,
                IsCorrectAnswer = a.IsCorrectAnswer,
                QuestionId = question.Id
            }).ToList();

            question.Answers = answers;

            await _questionRepo.CreateAsync(question);

            return new QuestionBankDetailDTO
            {
                Id = question.Id,
                Content = question.Content,
                Points = question.Points,
                Category = question.Category,
                IsDraft = question.IsDraft,
                Answers = answers.Select(a => new AnswerDetailDTO
                {
                    Id = a.Id,
                    Content = a.Content,
                    IsCorrectAnswer = a.IsCorrectAnswer
                }).ToList(),
                UsedInQuizCount = 0
            };
        }

        public async Task<List<QuestionBankResponseDTO>> GetQuestionsAsync(Guid creatorId, string? category = null, string? searchTerm = null)
        {
            
            var questions = await _questionRepo.GetByCreatorIdAsync(creatorId, category, searchTerm);

            return questions.Select(q => new QuestionBankResponseDTO
            {
                Id = q.Id,
                Content = q.Content,
                Points = q.Points,
                Category = q.Category,
                IsDraft = q.IsDraft,
                AnswerCount = q.Answers?.Count ?? 0,
                UsedInQuizCount = q.QuizQuestions?.Count ?? 0
            }).ToList();
        }

        public async Task<QuestionBankDetailDTO> GetQuestionDetailAsync(Guid questionId, Guid creatorId)
        {
            var question = await _questionRepo.GetByIdAsync(questionId);

            if (question == null)
                throw new InvalidDataException("Question not found");

            if (question.CreatorId != creatorId)
                throw new UnauthorizedAccessException("You don't have permission to access this question");

            return new QuestionBankDetailDTO
            {
                Id = question.Id,
                Content = question.Content,
                Points = question.Points,
                Category = question.Category,
                IsDraft = question.IsDraft,
                Answers = question.Answers?.Select(a => new AnswerDetailDTO
                {
                    Id = a.Id,
                    Content = a.Content,
                    IsCorrectAnswer = a.IsCorrectAnswer
                }).ToList() ?? new(),
                UsedInQuizCount = question.QuizQuestions?.Count ?? 0
            };
        }

        public async Task<QuestionBankDetailDTO> UpdateQuestionAsync(Guid questionId, UpdateQuestionBankDTO dto, Guid creatorId)
        {
            var question = await _questionRepo.GetByIdAsync(questionId);

            if (question == null)
                throw new InvalidDataException("Question not found");

            if (question.CreatorId != creatorId)
                throw new UnauthorizedAccessException("You don't have permission to update this question");

            // Update question fields
            if (dto.Content != null)
                question.Content = dto.Content;

            if (dto.Points.HasValue)
                question.Points = dto.Points.Value;

            if (dto.Category != null)
                question.Category = dto.Category;

            // Update answers
            if (dto.Answers != null)
            {
                var existingAnswers = question.Answers?.ToList() ?? new();

                // Process deletions
                var answersToDelete = dto.Answers.Where(a => a.IsDeleted && a.Id.HasValue).ToList();
                foreach (var answerDto in answersToDelete)
                {
                    var answerToRemove = existingAnswers.FirstOrDefault(a => a.Id == answerDto.Id);
                    if (answerToRemove != null)
                    {
                        _context.Answers.Remove(answerToRemove);
                        existingAnswers.Remove(answerToRemove);
                    }
                }

                // Process updates and creates
                foreach (var answerDto in dto.Answers.Where(a => !a.IsDeleted))
                {
                    if (answerDto.Id.HasValue)
                    {
                        // Update existing
                        var existingAnswer = existingAnswers.FirstOrDefault(a => a.Id == answerDto.Id);
                        if (existingAnswer != null)
                        {
                            if (answerDto.Content != null)
                                existingAnswer.Content = answerDto.Content;
                            if (answerDto.IsCorrectAnswer.HasValue)
                                existingAnswer.IsCorrectAnswer = answerDto.IsCorrectAnswer.Value;
                        }
                    }
                    else
                    {
                        // Create new
                        var newAnswer = new Answer
                        {
                            Id = Guid.NewGuid(),
                            Content = answerDto.Content ?? "",
                            IsCorrectAnswer = answerDto.IsCorrectAnswer ?? false,
                            QuestionId = questionId
                        };
                        _context.Answers.Add(newAnswer);
                        existingAnswers.Add(newAnswer);
                    }
                }

                // Validate at least one correct answer remains
                if (!existingAnswers.Any(a => a.IsCorrectAnswer))
                {
                    throw new InvalidDataException("Question must have at least one correct answer");
                }
            }

            await _questionRepo.UpdateAsync(question);

            // Reload to get updated data
            question = await _questionRepo.GetByIdAsync(questionId);

            return new QuestionBankDetailDTO
            {
                Id = question!.Id,
                Content = question.Content,
                Points = question.Points,
                Category = question.Category,
                IsDraft = question.IsDraft,
                Answers = question.Answers?.Select(a => new AnswerDetailDTO
                {
                    Id = a.Id,
                    Content = a.Content,
                    IsCorrectAnswer = a.IsCorrectAnswer
                }).ToList() ?? new(),
                UsedInQuizCount = question.QuizQuestions?.Count ?? 0
            };
        }

        public async Task DeleteQuestionAsync(Guid questionId, Guid creatorId)
        {
            var question = await _questionRepo.GetByIdAsync(questionId);

            if (question == null)
                throw new InvalidDataException("Question not found");

            if (question.CreatorId != creatorId)
                throw new UnauthorizedAccessException("You don't have permission to delete this question");

            // Check if question is being used in any quiz
            var usageCount = await _questionRepo.CountQuizzesUsingQuestionAsync(questionId);
            if (usageCount > 0)
            {
                throw new InvalidOperationException($"Cannot delete question. It is currently used in {usageCount} quiz(es). Please remove it from all quizzes first.");
            }

            await _questionRepo.DeleteAsync(questionId);
        }
    }
}
