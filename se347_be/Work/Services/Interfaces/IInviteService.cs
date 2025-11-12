using se347_be.Work.DTOs.Invite;

namespace se347_be.Work.Services.Interfaces
{
    public interface IInviteService
    {
        Task<InviteResponseDTO> SendInvitesAsync(Guid quizId, IFormFile excelFile, Guid creatorId);
        Task<InviteResponseDTO> SendInvitesFromListsAsync(Guid quizId, List<Guid> participantListIds, Guid creatorId);
    }
}
