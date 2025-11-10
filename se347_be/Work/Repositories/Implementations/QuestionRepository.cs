using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.Repositories.Interfaces;

namespace se347_be.Work.Repositories.Implementations
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly MyAppDbContext _db;

        public QuestionRepository(MyAppDbContext dbContext)
        {
            _db = dbContext;
        }

        public async Task<Guid> CreateQuestionAsync(Question question)
        {
            await _db.Questions.AddAsync(question);
            await _db.SaveChangesAsync();
            return question.Id;
        }

        public async Task<Question?> GetQuestionByIdAsync(Guid id)
        {
            return await _db.Questions.FindAsync(id);
        }

        public async Task<Question?> GetQuestionWithAnswersAsync(Guid id)
        {
            return await _db.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<List<Question>> GetQuestionsByQuizIdAsync(Guid quizId)
        {
            var questionIds = await _db.QuizQuestions
                .Where(qq => qq.QuizId == quizId)
                .Select(qq => qq.QuestionId)
                .ToListAsync();

            return await _db.Questions
                .Include(q => q.Answers)
                .Where(q => questionIds.Contains(q.Id))
                .ToListAsync();
        }

        public async Task UpdateQuestionAsync(Question question)
        {
            _db.Questions.Update(question);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteQuestionAsync(Guid id)
        {
            var question = await _db.Questions.FindAsync(id);
            if (question != null)
            {
                _db.Questions.Remove(question);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> IsQuestionInQuizAsync(Guid questionId, Guid quizId)
        {
            return await _db.QuizQuestions
                .AnyAsync(qq => qq.QuestionId == questionId && qq.QuizId == quizId);
        }
    }
}
