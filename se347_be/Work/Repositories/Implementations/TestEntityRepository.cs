using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Database.DbEntity;
using se347_be.Work.Repositories.Interfaces;

namespace se347_be.Work.Repositories.Implementations
{
    public class TestEntityRepository : ITestEntityRepository
    {
        private MyAppDbContext _db;
        public TestEntityRepository(MyAppDbContext db)
        {
            _db = db;
        }

        public async Task<string> AddNewEntityAsync(TestEntity newEntity)
        {
            await _db.TestEntities.AddAsync(newEntity);
            await _db.SaveChangesAsync();
            return newEntity.Id.ToString();
        }

        public async Task<List<TestEntity>> GetAllEntitiesAsync()
        {
            return await _db.TestEntities.ToListAsync();
        }

        public async Task<TestEntity> GetEntityByIdAsync(Guid id)
        {
            return await _db.TestEntities.Where(e => e.Id == id).FirstOrDefaultAsync();
        }
    }
}