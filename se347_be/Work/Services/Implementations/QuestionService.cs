using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs.Answer;
using se347_be.Work.DTOs.Question;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IAnswerRepository _answerRepository;
        private readonly IQuizRepository _quizRepository;

        public QuestionService(
            IQuestionRepository questionRepository,
            IAnswerRepository answerRepository,
            IQuizRepository quizRepository)
        {
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
            _quizRepository = quizRepository;
        }

        public async Task<QuestionResponseDTO> CreateQuestionAsync(Guid quizId, CreateQuestionDTO createQuestionDTO, Guid creatorId)
        {
            // Verify quiz ownership
            var isOwned = await _quizRepository.IsQuizOwnedByUserAsync(quizId, creatorId);
            if (!isOwned)
            {
                throw new UnauthorizedAccessException("You don't have permission to add questions to this quiz");
            }

            // Validate at least one correct answer
            if (!createQuestionDTO.Answers.Any(a => a.IsCorrectAnswer))
            {
                throw new InvalidDataException("At least one answer must be marked as correct");
            }

            var question = new Question
            {
                Id = Guid.NewGuid(),
                Content = createQuestionDTO.Content,
                QuizId = quizId
            };

            await _questionRepository.CreateQuestionAsync(question);

            // Create answers
            var answers = createQuestionDTO.Answers.Select(a => new Answer
            {
                Id = Guid.NewGuid(),
                Content = a.Content,
                IsCorrectAnswer = a.IsCorrectAnswer,
                QuestionId = question.Id
            }).ToList();

            await _answerRepository.CreateAnswersAsync(answers);

            return new QuestionResponseDTO
            {
                Id = question.Id,
                Content = question.Content,
                QuizId = question.QuizId,
                Answers = answers.Select(a => new AnswerResponseDTO
                {
                    Id = a.Id,
                    Content = a.Content,
                    IsCorrectAnswer = a.IsCorrectAnswer
                }).ToList()
            };
        }

        public async Task<List<QuestionResponseDTO>> GetQuestionsByQuizIdAsync(Guid quizId, Guid creatorId)
        {
            // Verify quiz ownership
            var isOwned = await _quizRepository.IsQuizOwnedByUserAsync(quizId, creatorId);
            if (!isOwned)
            {
                throw new UnauthorizedAccessException("You don't have permission to view questions of this quiz");
            }

            var questions = await _questionRepository.GetQuestionsByQuizIdAsync(quizId);

            return questions.Select(q => new QuestionResponseDTO
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
            }).ToList();
        }

        public async Task<QuestionResponseDTO> UpdateQuestionAsync(Guid quizId, Guid questionId, UpdateQuestionDTO updateQuestionDTO, Guid creatorId)
        {
            // Verify quiz ownership
            var isOwned = await _quizRepository.IsQuizOwnedByUserAsync(quizId, creatorId);
            if (!isOwned)
            {
                throw new UnauthorizedAccessException("You don't have permission to update questions of this quiz");
            }

            // Verify question belongs to quiz
            var isQuestionInQuiz = await _questionRepository.IsQuestionInQuizAsync(questionId, quizId);
            if (!isQuestionInQuiz)
            {
                throw new InvalidDataException("Question not found in this quiz");
            }

            var question = await _questionRepository.GetQuestionWithAnswersAsync(questionId);
            if (question == null)
            {
                throw new InvalidDataException("Question not found");
            }

            // Update content if provided
            if (!string.IsNullOrEmpty(updateQuestionDTO.Content))
            {
                question.Content = updateQuestionDTO.Content;
            }

            // Update answers if provided
            if (updateQuestionDTO.Answers != null && updateQuestionDTO.Answers.Any())
            {
                // Validate at least one correct answer
                if (!updateQuestionDTO.Answers.Any(a => a.IsCorrectAnswer))
                {
                    throw new InvalidDataException("At least one answer must be marked as correct");
                }

                // Delete old answers
                await _answerRepository.DeleteAnswersByQuestionIdAsync(questionId);

                // Create new answers
                var newAnswers = updateQuestionDTO.Answers.Select(a => new Answer
                {
                    Id = Guid.NewGuid(),
                    Content = a.Content,
                    IsCorrectAnswer = a.IsCorrectAnswer,
                    QuestionId = questionId
                }).ToList();

                await _answerRepository.CreateAnswersAsync(newAnswers);

                question.Answers = newAnswers;
            }

            await _questionRepository.UpdateQuestionAsync(question);

            return new QuestionResponseDTO
            {
                Id = question.Id,
                Content = question.Content,
                QuizId = question.QuizId,
                Answers = question.Answers?.Select(a => new AnswerResponseDTO
                {
                    Id = a.Id,
                    Content = a.Content,
                    IsCorrectAnswer = a.IsCorrectAnswer
                }).ToList()
            };
        }

        public async Task DeleteQuestionAsync(Guid quizId, Guid questionId, Guid creatorId)
        {
            // Verify quiz ownership
            var isOwned = await _quizRepository.IsQuizOwnedByUserAsync(quizId, creatorId);
            if (!isOwned)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete questions from this quiz");
            }

            // Verify question belongs to quiz
            var isQuestionInQuiz = await _questionRepository.IsQuestionInQuizAsync(questionId, quizId);
            if (!isQuestionInQuiz)
            {
                throw new InvalidDataException("Question not found in this quiz");
            }

            await _questionRepository.DeleteQuestionAsync(questionId);
        }
    }
}
