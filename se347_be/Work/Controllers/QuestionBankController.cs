using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using se347_be.Work.DTOs.QuestionBank;
using se347_be.Work.Services.Interfaces;
using System.Security.Claims;

namespace se347_be.Work.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuestionBankController : ControllerBase
    {
        private readonly IQuestionBankService _questionBankService;

        public QuestionBankController(IQuestionBankService questionBankService)
        {
            _questionBankService = questionBankService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        [HttpPost]
        public async Task<ActionResult<QuestionBankDetailDTO>> CreateQuestion([FromBody] CreateQuestionBankDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _questionBankService.CreateQuestionAsync(dto, userId);
                return Ok(result);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<QuestionBankResponseDTO>>> GetQuestions(
            [FromQuery] string? category = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _questionBankService.GetQuestionsAsync(userId, category, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet("{questionId}")]
        public async Task<ActionResult<QuestionBankDetailDTO>> GetQuestionDetail([FromRoute] Guid questionId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _questionBankService.GetQuestionDetailAsync(questionId, userId);
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

        [HttpPut("{questionId}")]
        public async Task<ActionResult<QuestionBankDetailDTO>> UpdateQuestion(
            [FromRoute] Guid questionId,
            [FromBody] UpdateQuestionBankDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _questionBankService.UpdateQuestionAsync(questionId, dto, userId);
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

        [HttpDelete("{questionId}")]
        public async Task<ActionResult> DeleteQuestion([FromRoute] Guid questionId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _questionBankService.DeleteQuestionAsync(questionId, userId);
                return Ok(new { Message = "Question deleted successfully" });
            }
            catch (InvalidDataException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
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
