using se347_be.Work.Database.Entity;

namespace se347_be.Work.Repositories.Interfaces
{
    public interface IQuestionBankRepository
    {
        Task<Question> CreateAsync(Question question);
        Task<List<Question>> GetByCreatorIdAsync(Guid creatorId, string? category = null, string? searchTerm = null);
        Task<Question?> GetByIdAsync(Guid questionId);
        Task UpdateAsync(Question question);
        Task DeleteAsync(Guid questionId);
        Task<int> CountQuizzesUsingQuestionAsync(Guid questionId);
        Task<bool> IsOwnerAsync(Guid questionId, Guid creatorId);
    }
}
