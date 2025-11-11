using se347_be.Work.DTOs.Statistics;

namespace se347_be.Work.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<QuizStatisticsDTO> GetQuizStatisticsAsync(Guid quizId, Guid creatorId);
        Task<List<ParticipationListDTO>> GetParticipationsAsync(Guid quizId, Guid creatorId);
        Task<ParticipationDetailDTO> GetParticipationDetailAsync(Guid participationId, Guid creatorId);
        Task<byte[]> ExportToExcelAsync(Guid quizId, Guid creatorId);
    }
}
