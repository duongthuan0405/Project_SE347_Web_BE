using Microsoft.AspNetCore.Http;
using se347_be.Work.DTOs.ParticipantList;

namespace se347_be.Work.Services.Interfaces
{
    public interface IParticipantListService
    {
        Task<ParticipantListResponseDTO> CreateListAsync(CreateParticipantListDTO dto, Guid creatorId);
        Task<List<ParticipantListResponseDTO>> GetListsByCreatorAsync(Guid creatorId);
        Task<ParticipantListDetailDTO> GetListDetailAsync(Guid listId, Guid userId);
        Task<ParticipantListResponseDTO> UpdateListAsync(Guid listId, UpdateParticipantListDTO dto, Guid userId);
        Task DeleteListAsync(Guid listId, Guid userId);
        Task<ImportParticipantsResponseDTO> ImportParticipantsAsync(Guid listId, IFormFile file, Guid userId);
        Task<ParticipantResponseDTO> AddParticipantAsync(Guid listId, AddParticipantDTO dto, Guid userId);
    }
}
