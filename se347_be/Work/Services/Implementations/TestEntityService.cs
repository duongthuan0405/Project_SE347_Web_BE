using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using se347_be.Database.DbEntity;
using se347_be.DTOs;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class TestEntityService : ITestEntityService
    {
        ITestEntityRepository _testEntityRepository;
        public TestEntityService(ITestEntityRepository testEntityRepository)
        {
            _testEntityRepository = testEntityRepository;
        }

        public async Task<string> AddNewEntity(TestEntityDTO testEntityDTO)
        {
            TestEntity newEntity = new TestEntity(testEntityDTO.Name ?? "");
            return await _testEntityRepository.AddNewEntityAsync(newEntity);
        }

        public async Task<List<TestEntityDTO>> GetAllEntities()
        {
            var list = await _testEntityRepository.GetAllEntitiesAsync();
            return list.Select(entity => new TestEntityDTO(entity.Id.ToString(), entity.Name)).ToList();   
        }

        public async Task<TestEntityDTO> GetEntityById(string id)
        {
            Guid key;
            try
            {
                key = Guid.Parse(id);
            }
            catch (Exception)
            {
                throw new Exception("Invalid ID format");
            }

            TestEntity result = await _testEntityRepository.GetEntityByIdAsync(key);
            return new TestEntityDTO(result.Id.ToString(), result.Name);
        }
    }
}