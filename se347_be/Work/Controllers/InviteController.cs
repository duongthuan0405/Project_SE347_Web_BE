using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using se347_be.Work.DTOs.Invite;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Controllers
{
    [ApiController]
    [Route("api/Quiz/{quizId}/[controller]")]
    [Authorize]
    public class InviteController : ControllerBase
    {
        private readonly IInviteService _inviteService;

        public InviteController(IInviteService inviteService)
        {
            _inviteService = inviteService;
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
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<InviteResponseDTO>> SendInvites(
            [FromRoute] Guid quizId,
            IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { Message = "Please upload a valid Excel file" });
                }

                var userId = GetCurrentUserId();
                var result = await _inviteService.SendInvitesAsync(quizId, file, userId);
                
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

        [HttpPost("from-lists")]
        public async Task<ActionResult<InviteResponseDTO>> SendInvitesFromLists(
            [FromRoute] Guid quizId,
            [FromBody] InviteFromListDTO dto)
        {
            try
            {
                if (dto.ParticipantListIds == null || !dto.ParticipantListIds.Any())
                {
                    return BadRequest(new { Message = "Please provide at least one participant list" });
                }

                var userId = GetCurrentUserId();
                var result = await _inviteService.SendInvitesFromListsAsync(quizId, dto.ParticipantListIds, userId);
                
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
