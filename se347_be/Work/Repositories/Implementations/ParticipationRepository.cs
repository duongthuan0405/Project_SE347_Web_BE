using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Work.Database.Entities;
using se347_be.Work.Repositories.Interfaces;

namespace se347_be.Work.Repositories.Implementations
{
    public class ParticipationRepository : IParticipationRepository
    {
        private readonly MyAppDbContext _db;

        public ParticipationRepository(MyAppDbContext dbContext)
        {
            _db = dbContext;
        }

        public async Task<List<QuizParticipation>> GetParticipationsByQuizIdAsync(Guid quizId)
        {
            return await _db.QuizParticipations
                .Include(p => p.AnswerSelections!)
                    .ThenInclude(a => a.Answer)
                .Where(p => p.QuizId == quizId)
                .ToListAsync();
        }

        public async Task<QuizParticipation?> GetParticipationByIdAsync(Guid participationId)
        {
            return await _db.QuizParticipations.FindAsync(participationId);
        }

        public async Task<QuizParticipation?> GetParticipationWithDetailsAsync(Guid participationId)
        {
            return await _db.QuizParticipations
                .Include(p => p.Quiz!)
                    .ThenInclude(q => q.Questions!)
                        .ThenInclude(q => q.Answers!)
                .Include(p => p.AnswerSelections!)
                    .ThenInclude(a => a.Answer)
                        .ThenInclude(a => a.Question!)
                .FirstOrDefaultAsync(p => p.Id == participationId);
        }
    }
}
