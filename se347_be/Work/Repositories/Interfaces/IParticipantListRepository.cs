using se347_be.Work.Database.Entity;

namespace se347_be.Work.Repositories.Interfaces
{
    public interface IParticipantListRepository
    {
        Task<ParticipantList> CreateAsync(ParticipantList participantList);
        Task<List<ParticipantList>> GetByCreatorIdAsync(Guid creatorId);
        Task<ParticipantList?> GetByIdAsync(Guid id);
        Task<ParticipantList?> GetByIdWithParticipantsAsync(Guid id);
        Task<ParticipantList> UpdateAsync(ParticipantList participantList);
        Task DeleteAsync(Guid id);
        Task<bool> IsOwnerAsync(Guid listId, Guid userId);
    }
}
