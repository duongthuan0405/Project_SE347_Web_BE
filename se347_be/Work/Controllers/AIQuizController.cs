using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using se347_be.Work.DTOs.AI;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Controllers
{
    [ApiController]
    [Route("api/Quiz/{quizId}/[controller]")]
    [Authorize]
    public class AIQuizController : ControllerBase
    {
        private readonly IGeminiAIService _geminiService;
        private readonly IDocumentProcessorService _docProcessor;
        private readonly IQuizService _quizService;
        private readonly ILogger<AIQuizController> _logger;

        public AIQuizController(
            IGeminiAIService geminiService,
            IDocumentProcessorService docProcessor,
            IQuizService quizService,
            ILogger<AIQuizController> logger)
        {
            _geminiService = geminiService;
            _docProcessor = docProcessor;
            _quizService = quizService;
            _logger = logger;
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

        [HttpPost("generate")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<GenerateQuizResponseDTO>> GenerateQuestions(
            [FromRoute] Guid quizId,
            GenerateQuizFormDTO request)
        {
            try
            {
                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(new { Message = "Please upload a valid document file" });
                }

                if (!_docProcessor.IsSupportedFileType(request.File.FileName))
                {
                    return BadRequest(new { Message = "Unsupported file type. Please upload .txt, .docx, or .pdf file" });
                }

                var userId = GetCurrentUserId();

                // Verify quiz ownership
                var quiz = await _quizService.GetQuizDetailAsync(quizId, userId);
                if (quiz == null)
                {
                    return NotFound(new { Message = "Quiz not found or you don't have permission" });
                }

                _logger.LogInformation("Extracting text from file: {FileName}", request.File.FileName);
                var textContent = await _docProcessor.ExtractTextFromFileAsync(request.File);

                if (string.IsNullOrWhiteSpace(textContent) || textContent.Length < 100)
                {
                    return BadRequest(new { Message = "Document content is too short or empty. Please provide a document with meaningful content." });
                }

                _logger.LogInformation("Generating {Count} questions using Gemini AI and saving to Question Bank", request.NumberOfQuestions);
                
                // Generate and save to Question Bank + Auto-link to Quiz
                var result = await _geminiService.GenerateAndSaveQuestionsAsync(
                    quizId,
                    textContent,
                    request.File.FileName,
                    request.NumberOfQuestions,
                    userId,
                    request.AdditionalInstructions
                );

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
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { 
                    Message = "Failed to generate questions",
                    Details = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in AI quiz generation");
                return StatusCode(StatusCodes.Status500InternalServerError, new { 
                    Message = "An unexpected error occurred",
                    Details = ex.Message 
                });
            }
        }
    }
}
