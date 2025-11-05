using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.Repositories.Interfaces;

namespace se347_be.Work.Repositories.Implementations
{
    public class QuizRepository : IQuizRepository
    {
        private readonly MyAppDbContext _db;

        public QuizRepository(MyAppDbContext dbContext)
        {
            _db = dbContext;
        }

        public async Task<Guid> CreateQuizAsync(Quiz quiz)
        {
            await _db.Quizzes.AddAsync(quiz);
            await _db.SaveChangesAsync();
            return quiz.Id;
        }

        public async Task<Quiz?> GetQuizByIdAsync(Guid id)
        {
            return await _db.Quizzes
                .Include(q => q.Participations)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Quiz?> GetQuizWithQuestionsAsync(Guid id)
        {
            return await _db.Quizzes
                .Include(q => q.Questions!)
                    .ThenInclude(q => q.Answers!)
                .Include(q => q.Participations)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<List<Quiz>> GetQuizzesByCreatorIdAsync(Guid creatorId)
        {
            return await _db.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.Participations)
                .Where(q => q.CreatorId == creatorId)
                .OrderByDescending(q => q.CreateAt)
                .ToListAsync();
        }

        public async Task UpdateQuizAsync(Quiz quiz)
        {
            _db.Quizzes.Update(quiz);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteQuizAsync(Guid id)
        {
            var quiz = await _db.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                _db.Quizzes.Remove(quiz);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> IsQuizOwnedByUserAsync(Guid quizId, Guid userId)
        {
            return await _db.Quizzes
                .AnyAsync(q => q.Id == quizId && q.CreatorId == userId);
        }
    }
}
