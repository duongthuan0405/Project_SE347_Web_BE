using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using se347_be.DTOs;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestEntitiesController : ControllerBase
    {
        ITestEntityService _testEntityService;
        public TestEntitiesController(ITestEntityService testEntityService)
        {
            _testEntityService = testEntityService;
        }

        [HttpPost]
        public async Task<ActionResult<string>> AddNewTestEntity([FromBody] TestEntityDTO testEntityDTO)
        {
            try
            {
                var result = await _testEntityService.AddNewEntity(testEntityDTO);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<TestEntityDTO>>> GetTestEntities()
        {
            try
            {
                return await _testEntityService.GetAllEntities();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("/{id}")]
        public async Task<ActionResult<TestEntityDTO>> GetTestEntityById([FromRoute] string id)
        {
            try
            {
                var r = await _testEntityService.GetEntityById(id);

                return Ok(r); // Placeholder
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}