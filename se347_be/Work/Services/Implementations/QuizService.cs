using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs.Answer;
using se347_be.Work.DTOs.Question;
using se347_be.Work.DTOs.Quiz;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;

        public QuizService(IQuizRepository quizRepository)
        {
            _quizRepository = quizRepository;
        }

        public async Task<QuizResponseDTO> CreateQuizAsync(CreateQuizDTO createQuizDTO, Guid creatorId)
        {
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
                CreatorId = q.CreatorId,
                TotalQuestions = q.Questions?.Count ?? 0,
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
                CreatorId = quiz.CreatorId,
                Questions = quiz.Questions?.Select(q => new QuestionResponseDTO
                {
                    Id = q.Id,
                    Content = q.Content,
                    QuizId = q.QuizId,
                    Answers = q.Answers?.Select(a => new AnswerResponseDTO
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
    }
}
