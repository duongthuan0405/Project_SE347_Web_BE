using se347_be.Work.Database.Entity;

namespace se347_be.Work.Repositories.Interfaces
{
    public interface IQuizRepository
    {
        Task<Guid> CreateQuizAsync(Quiz quiz);
        Task<Quiz?> GetQuizByIdAsync(Guid id);
        Task<Quiz?> GetQuizWithQuestionsAsync(Guid id);
        Task<List<Quiz>> GetQuizzesByCreatorIdAsync(Guid creatorId);
        Task UpdateQuizAsync(Quiz quiz);
        Task DeleteQuizAsync(Guid id);
        Task<bool> IsQuizOwnedByUserAsync(Guid quizId, Guid userId);
    }
}
