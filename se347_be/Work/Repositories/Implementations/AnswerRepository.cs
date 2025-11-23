using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.Repositories.Interfaces;

namespace se347_be.Work.Repositories.Implementations
{
    public class AnswerRepository : IAnswerRepository
    {
        private readonly MyAppDbContext _db;

        public AnswerRepository(MyAppDbContext dbContext)
        {
            _db = dbContext;
        }

        public async Task CreateAnswersAsync(List<Answer> answers)
        {
            await _db.Answers.AddRangeAsync(answers);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAnswersByQuestionIdAsync(Guid questionId)
        {
            var answers = await _db.Answers
                .Where(a => a.QuestionId == questionId)
                .ToListAsync();

            _db.Answers.RemoveRange(answers);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Answer>> GetAnswersByQuestionIdAsync(Guid questionId)
        {
            return await _db.Answers
                .Where(a => a.QuestionId == questionId)
                .ToListAsync();
        }

        public async Task UpdateAnswer(List<Answer> updatedAnswers)
        {
            _db.Answers.UpdateRange(updatedAnswers);
            await _db.SaveChangesAsync();
           
        }
    }
}
