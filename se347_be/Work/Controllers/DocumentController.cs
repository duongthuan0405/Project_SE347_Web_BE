using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs.Document;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Controllers
{
    [ApiController]
    [Route("api/Quiz/{quizId}/[controller]")]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly MyAppDbContext _context;
        private readonly IQuizService _quizService;
        private readonly IDocumentProcessorService _docProcessor;
        private readonly ILogger<DocumentController> _logger;
        private readonly string _uploadPath;

        public DocumentController(
            MyAppDbContext context,
            IQuizService quizService,
            IDocumentProcessorService docProcessor,
            ILogger<DocumentController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _quizService = quizService;
            _docProcessor = docProcessor;
            _logger = logger;
            
            // Get upload path from configuration or use default
            _uploadPath = configuration["FileUpload:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads", "documents");
            
            // Create directory if not exists
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
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

        /// <summary>
        /// Upload document to quiz
        /// Creates QuizSourceDocument record in database
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(DocumentResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentResponseDTO>> UploadDocument(
            [FromRoute] Guid quizId,
            IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { Message = "Please upload a valid file" });
                }

                var userId = GetCurrentUserId();

                // Verify quiz exists and user owns it
                var quiz = await _context.Quizzes
                    .FirstOrDefaultAsync(q => q.Id == quizId && q.CreatorId == userId);

                if (quiz == null)
                {
                    return NotFound(new { Message = "Quiz not found or you don't have permission" });
                }

                // Validate file type
                if (!_docProcessor.IsSupportedFileType(file.FileName))
                {
                    return BadRequest(new { Message = "Unsupported file type. Supported: .txt, .pdf, .docx" });
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadPath, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File uploaded: {FilePath}", filePath);

                // Create QuizSourceDocument record
                var document = new QuizSourceDocument
                {
                    Id = Guid.NewGuid(),
                    QuizId = quizId,
                    FileName = file.FileName,
                    StorageUrl = filePath,
                    Status = "Uploaded",
                    UploadAt = DateTime.UtcNow
                };

                _context.QuizSourceDocuments.Add(document);
                await _context.SaveChangesAsync();

                return Ok(new DocumentResponseDTO
                {
                    Id = document.Id,
                    QuizId = document.QuizId,
                    FileName = document.FileName,
                    StorageUrl = document.StorageUrl,
                    Status = document.Status,
                    UploadAt = document.UploadAt
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(StatusCodes.Status500InternalServerError, new { 
                    Message = "Failed to upload document",
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get all documents for a quiz
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<DocumentResponseDTO>>> GetDocuments([FromRoute] Guid quizId)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Verify quiz ownership
                var quiz = await _context.Quizzes
                    .FirstOrDefaultAsync(q => q.Id == quizId && q.CreatorId == userId);

                if (quiz == null)
                {
                    return NotFound(new { Message = "Quiz not found or you don't have permission" });
                }

                var documents = await _context.QuizSourceDocuments
                    .Where(d => d.QuizId == quizId)
                    .Select(d => new DocumentResponseDTO
                    {
                        Id = d.Id,
                        QuizId = d.QuizId,
                        FileName = d.FileName,
                        StorageUrl = d.StorageUrl,
                        Status = d.Status,
                        UploadAt = d.UploadAt
                    })
                    .ToListAsync();

                return Ok(documents);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a document
        /// </summary>
        [HttpDelete("{documentId}")]
        public async Task<ActionResult> DeleteDocument(
            [FromRoute] Guid quizId,
            [FromRoute] Guid documentId)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Verify quiz ownership
                var quiz = await _context.Quizzes
                    .FirstOrDefaultAsync(q => q.Id == quizId && q.CreatorId == userId);

                if (quiz == null)
                {
                    return NotFound(new { Message = "Quiz not found or you don't have permission" });
                }

                var document = await _context.QuizSourceDocuments
                    .FirstOrDefaultAsync(d => d.Id == documentId && d.QuizId == quizId);

                if (document == null)
                {
                    return NotFound(new { Message = "Document not found" });
                }

                // Delete file from disk
                if (System.IO.File.Exists(document.StorageUrl))
                {
                    System.IO.File.Delete(document.StorageUrl);
                }

                // Delete record
                _context.QuizSourceDocuments.Remove(document);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Document deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }
    }
}
