using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using se347_be.Work.DTOs.Statistics;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
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

        [HttpGet("quiz/{quizId}")]
        public async Task<ActionResult<QuizStatisticsDTO>> GetQuizStatistics([FromRoute] Guid quizId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var statistics = await _statisticsService.GetQuizStatisticsAsync(quizId, userId);
                return Ok(statistics);
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

        [HttpGet("quiz/{quizId}/participations")]
        public async Task<ActionResult<List<ParticipationListDTO>>> GetParticipations([FromRoute] Guid quizId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var participations = await _statisticsService.GetParticipationsAsync(quizId, userId);
                return Ok(participations);
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

        [HttpGet("participation/{id}")]
        public async Task<ActionResult<ParticipationDetailDTO>> GetParticipationDetail([FromRoute] Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var detail = await _statisticsService.GetParticipationDetailAsync(id, userId);
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

        [HttpGet("quiz/{quizId}/export/excel")]
        public async Task<IActionResult> ExportToExcel([FromRoute] Guid quizId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var fileContent = await _statisticsService.ExportToExcelAsync(quizId, userId);
                
                return File(
                    fileContent,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Quiz_Results_{quizId}_{DateTime.Now:yyyyMMdd}.xlsx"
                );
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
    }
}
