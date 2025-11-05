using se347_be.Work.Database.Entity;

namespace se347_be.Work.Repositories.Interfaces
{
    public interface IQuestionRepository
    {
        Task<Guid> CreateQuestionAsync(Question question);
        Task<Question?> GetQuestionByIdAsync(Guid id);
        Task<Question?> GetQuestionWithAnswersAsync(Guid id);
        Task<List<Question>> GetQuestionsByQuizIdAsync(Guid quizId);
        Task UpdateQuestionAsync(Question question);
        Task DeleteQuestionAsync(Guid id);
        Task<bool> IsQuestionInQuizAsync(Guid questionId, Guid quizId);
    }
}
