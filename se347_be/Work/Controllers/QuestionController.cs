using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using se347_be.Work.DTOs.Question;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Controllers
{
    [ApiController]
    [Route("api/Quiz/{quizId}/[controller]")]
    [Authorize]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
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
        public async Task<ActionResult<QuestionResponseDTO>> CreateQuestion(
            [FromRoute] Guid quizId,
            [FromBody] CreateQuestionDTO createQuestionDTO)
        {
            try
            {
                var userId = GetCurrentUserId();
                var question = await _questionService.CreateQuestionAsync(quizId, createQuestionDTO, userId);
                return Ok(question);
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

        [HttpGet]
        public async Task<ActionResult<List<QuestionResponseDTO>>> GetQuestions([FromRoute] Guid quizId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var questions = await _questionService.GetQuestionsByQuizIdAsync(quizId, userId);
                return Ok(questions);
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
        public async Task<ActionResult<QuestionResponseDTO>> UpdateQuestion(
            [FromRoute] Guid quizId,
            [FromRoute] Guid id,
            [FromBody] UpdateQuestionDTO updateQuestionDTO)
        {
            try
            {
                var userId = GetCurrentUserId();
                var question = await _questionService.UpdateQuestionAsync(quizId, id, updateQuestionDTO, userId);
                return Ok(question);
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteQuestion(
            [FromRoute] Guid quizId,
            [FromRoute] Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _questionService.DeleteQuestionAsync(quizId, id, userId);
                return Ok(new { Message = "Question deleted successfully" });
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
