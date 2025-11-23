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

        [HttpGet("{id}/link")]
        public async Task<ActionResult<DTOs.Quiz.GenerateQuizLinkDTO>> GetQuizLink([FromRoute] Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var quiz = await _quizService.GetQuizDetailAsync(id, userId);
                
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var publicLink = $"{baseUrl}/quiz/{id}";

                return Ok(new DTOs.Quiz.GenerateQuizLinkDTO
                {
                    QuizId = quiz.Id,
                    PublicLink = publicLink,
                    AccessCode = quiz.AccessCode,
                    RequiresAccessCode = !string.IsNullOrEmpty(quiz.AccessCode)
                });
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

        [HttpPut("{id}/reset-access-code")]
        public async Task<ActionResult<DTOs.Quiz.GenerateAccessCodeDTO>> ResetAccessCode([FromRoute] Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var quiz = await _quizService.GetQuizDetailAsync(id, userId);
                
                // Generate new random 6-character access code
                const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
                var random = new Random();
                var accessCode = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                
                await _quizService.UpdateQuizAsync(id, new UpdateQuizDTO 
                { 
                    AccessCode = accessCode 
                }, userId);

                return Ok(new DTOs.Quiz.GenerateAccessCodeDTO
                {
                    AccessCode = accessCode,
                    Message = "Access code reset successfully"
                });
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

        [HttpPut("{quizId}/questions")]
        public async Task<ActionResult> CreateQuestionInQuiz(
            [FromRoute] Guid quizId,
            [FromBody] CreateQuestionInQuizDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _quizService.CreateQuestionInQuizAsync(quizId, dto, userId);
                return Ok(new { 
                    Message = "Question created and added to quiz successfully",
                });
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

        [HttpPost("{quizId}/add-question")]
        public async Task<ActionResult> AddQuestionToQuiz(
            [FromRoute] Guid quizId,
            [FromBody] AddQuestionToQuizDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _quizService.AddQuestionToQuizAsync(quizId, dto.QuestionId, userId);
                return Ok(new { Message = "Question added to quiz successfully" });
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

        [HttpDelete("{quizId}/remove-question/{questionId}")]
        public async Task<ActionResult> RemoveQuestionFromQuiz(
            [FromRoute] Guid quizId,
            [FromRoute] Guid questionId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _quizService.RemoveQuestionFromQuizAsync(quizId, questionId, userId);
                return Ok(new { Message = "Question removed from quiz successfully" });
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

        /// <summary>
        /// Generate questions from uploaded document and add to existing quiz
        /// Flow: 1. Create Quiz → 2. Upload Document → 3. Generate Questions
        /// Questions will be created with category = "AI"
        /// Auto-saved to Question Bank when quiz is published
        /// </summary>
        [HttpPost("{quizId}/generate-questions-from-document")]
        public async Task<ActionResult<QuizDetailDTO>> GenerateQuestionsFromDocument(
            [FromRoute] Guid quizId,
            [FromBody] GenerateQuestionsFromDocumentDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var quiz = await _quizService.GenerateQuestionsFromDocumentAsync(quizId, dto, userId);
                return Ok(quiz);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"AI Generation failed: {ex.Message}" });
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

        /// <summary>
        /// Manually save quiz questions to bank (if not auto-saved yet)
        /// </summary>
        [HttpPost("{quizId}/save-to-bank")]
        public async Task<ActionResult> SaveQuizQuestionsToBank([FromRoute] Guid quizId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _quizService.SaveQuizQuestionsToBankAsync(quizId, userId);
                return Ok(new { Message = "Questions saved to bank successfully" });
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

// DTO for add-question endpoint
namespace se347_be.Work.DTOs.Quiz
{
    public class AddQuestionToQuizDTO
    {
        public Guid QuestionId { get; set; }
    }
}
