using se347_be.Work.Database.Entity;

namespace se347_be.Work.Repositories.Interfaces
{
    public interface IAnswerRepository
    {
        Task CreateAnswersAsync(List<Answer> answers);
        Task DeleteAnswersByQuestionIdAsync(Guid questionId);
        Task<List<Answer>> GetAnswersByQuestionIdAsync(Guid questionId);
    }
}
