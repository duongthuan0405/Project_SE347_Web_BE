using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.Repositories.Interfaces;

namespace se347_be.Work.Repositories.Implementations
{
    public class QuestionBankRepository : IQuestionBankRepository
    {
        private readonly MyAppDbContext _context;

        public QuestionBankRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<Question> CreateAsync(Question question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<List<Question>> GetByCreatorIdAsync(Guid creatorId, string? category = null, string? searchTerm = null)
        {
            var query = _context.Questions
                .Include(q => q.Answers)
                .Include(q => q.QuizQuestions)
                .Where(q => q.CreatorId == creatorId);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(q => q.Category == category);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(q => q.Content.Contains(searchTerm));

            return await query.OrderByDescending(q => q.Id).ToListAsync();
        }

        public async Task<Question?> GetByIdAsync(Guid questionId)
        {
            return await _context.Questions
                .Include(q => q.Answers)
                .Include(q => q.QuizQuestions)
                .FirstOrDefaultAsync(q => q.Id == questionId);
        }

        public async Task UpdateAsync(Question question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid questionId)
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> CountQuizzesUsingQuestionAsync(Guid questionId)
        {
            return await _context.QuizQuestions
                .Where(qq => qq.QuestionId == questionId)
                .CountAsync();
        }

        public async Task<bool> IsOwnerAsync(Guid questionId, Guid creatorId)
        {
            return await _context.Questions
                .AnyAsync(q => q.Id == questionId && q.CreatorId == creatorId);
        }
    }
}
