using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using se347_be.Work.DTOs.ParticipantList;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParticipantListController : ControllerBase
    {
        private readonly IParticipantListService _participantListService;

        public ParticipantListController(IParticipantListService participantListService)
        {
            _participantListService = participantListService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }

            return userId;
        }

        [HttpPost]
        public async Task<ActionResult<ParticipantListResponseDTO>> CreateList([FromBody] CreateParticipantListDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _participantListService.CreateListAsync(dto, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<ParticipantListResponseDTO>>> GetLists()
        {
            try
            {
                var userId = GetCurrentUserId();
                var lists = await _participantListService.GetListsByCreatorAsync(userId);
                return Ok(lists);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ParticipantListDetailDTO>> GetListDetail([FromRoute] Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var detail = await _participantListService.GetListDetailAsync(id, userId);
                return Ok(detail);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ParticipantListResponseDTO>> UpdateList(
            [FromRoute] Guid id,
            [FromBody] UpdateParticipantListDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _participantListService.UpdateListAsync(id, dto, userId);
                return Ok(result);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteList([FromRoute] Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _participantListService.DeleteListAsync(id, userId);
                return Ok(new { Message = "Participant list deleted successfully" });
            }
            catch (InvalidDataException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpPost("{id}/import")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImportParticipantsResponseDTO>> ImportParticipants(
            [FromRoute] Guid id,
            IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { Message = "Please upload a valid Excel file" });
                }

                var userId = GetCurrentUserId();
                var result = await _participantListService.ImportParticipantsAsync(id, file, userId);
                
                return Ok(result);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpPost("{listId}/participant")]
        public async Task<ActionResult<ParticipantResponseDTO>> AddParticipant(
            [FromRoute] Guid listId,
            [FromBody] AddParticipantDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _participantListService.AddParticipantAsync(listId, dto, userId);
                return Ok(result);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }
    }
}
