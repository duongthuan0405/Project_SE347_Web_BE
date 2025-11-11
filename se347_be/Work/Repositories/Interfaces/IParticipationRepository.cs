using se347_be.Work.Database.Entities;

namespace se347_be.Work.Repositories.Interfaces
{
    public interface IParticipationRepository
    {
        Task<List<QuizParticipation>> GetParticipationsByQuizIdAsync(Guid quizId);
        Task<QuizParticipation?> GetParticipationByIdAsync(Guid participationId);
        Task<QuizParticipation?> GetParticipationWithDetailsAsync(Guid participationId);
    }
}
