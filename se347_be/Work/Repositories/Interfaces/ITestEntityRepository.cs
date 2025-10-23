using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using se347_be.Database.DbEntity;

namespace se347_be.Work.Repositories.Interfaces
{
    public interface ITestEntityRepository
    {
        public Task<string> AddNewEntityAsync(TestEntity newEntity);
        public Task<List<TestEntity>> GetAllEntitiesAsync();
        public Task<TestEntity> GetEntityByIdAsync(Guid id);
    }
}