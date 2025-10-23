using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using se347_be.DTOs;

namespace se347_be.Work.Services.Interfaces
{
    public interface ITestEntityService
    {
        public Task<string> AddNewEntity(TestEntityDTO testEntityDTO);
        public Task<List<TestEntityDTO>> GetAllEntities();
        public Task<TestEntityDTO> GetEntityById(string id);
    }
}