using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.Repositories.Interfaces;

namespace se347_be.Work.Repositories.Implementations
{
    public class ParticipantListRepository : IParticipantListRepository
    {
        private readonly MyAppDbContext _context;

        public ParticipantListRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<ParticipantList> CreateAsync(ParticipantList participantList)
        {
            _context.ParticipantLists.Add(participantList);
            await _context.SaveChangesAsync();
            return participantList;
        }

        public async Task<List<ParticipantList>> GetByCreatorIdAsync(Guid creatorId)
        {
            return await _context.ParticipantLists
                .Where(pl => pl.CreatorId == creatorId)
                .Include(pl => pl.Participants)
                .OrderByDescending(pl => pl.Id)
                .ToListAsync();
        }

        public async Task<ParticipantList?> GetByIdAsync(Guid id)
        {
            return await _context.ParticipantLists.FindAsync(id);
        }

        public async Task<ParticipantList?> GetByIdWithParticipantsAsync(Guid id)
        {
            return await _context.ParticipantLists
                .Include(pl => pl.Participants)
                .FirstOrDefaultAsync(pl => pl.Id == id);
        }

        public async Task<ParticipantList> UpdateAsync(ParticipantList participantList)
        {
            _context.ParticipantLists.Update(participantList);
            await _context.SaveChangesAsync();
            return participantList;
        }

        public async Task DeleteAsync(Guid id)
        {
            var list = await _context.ParticipantLists.FindAsync(id);
            if (list != null)
            {
                _context.ParticipantLists.Remove(list);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsOwnerAsync(Guid listId, Guid userId)
        {
            return await _context.ParticipantLists
                .AnyAsync(pl => pl.Id == listId && pl.CreatorId == userId);
        }
    }
}
