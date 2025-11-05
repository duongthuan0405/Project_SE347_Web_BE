using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using se347_be.Work.DTOs.Quiz;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
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
        public async Task<ActionResult<QuizResponseDTO>> CreateQuiz([FromBody] CreateQuizDTO createQuizDTO)
        {
            try
            {
                var userId = GetCurrentUserId();
                var quiz = await _quizService.CreateQuizAsync(createQuizDTO, userId);
                return Ok(quiz);
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
        public async Task<ActionResult<List<QuizResponseDTO>>> GetQuizzes()
        {
            try
            {
                var userId = GetCurrentUserId();
                var quizzes = await _quizService.GetQuizzesByCreatorAsync(userId);
                return Ok(quizzes);
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
        public async Task<ActionResult<QuizDetailDTO>> GetQuizDetail([FromRoute] Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var quiz = await _quizService.GetQuizDetailAsync(id, userId);
                return Ok(quiz);
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
        public async Task<ActionResult<QuizResponseDTO>> UpdateQuiz([FromRoute] Guid id, [FromBody] UpdateQuizDTO updateQuizDTO)
        {
            try
            {
                var userId = GetCurrentUserId();
                var quiz = await _quizService.UpdateQuizAsync(id, updateQuizDTO, userId);
                return Ok(quiz);
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
        public async Task<ActionResult> DeleteQuiz([FromRoute] Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _quizService.DeleteQuizAsync(id, userId);
                return Ok(new { Message = "Quiz deleted successfully" });
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
