using Microsoft.AspNetCore.Mvc;
using se347_be.Work.DTOs.Participant;
using se347_be.Work.Exceptions;
using se347_be.Work.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace se347_be.Work.Controllers
{
    [ApiController]
    [Route("api/Public/Quiz")]
    public class PublicQuizController : ControllerBase
    {
        private readonly IParticipantQuizService _participantQuizService;

        public PublicQuizController(IParticipantQuizService participantQuizService)
        {
            _participantQuizService = participantQuizService;
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

        [HttpGet("{quizId}/info")]
        public async Task<ActionResult<QuizInfoDTO>> GetQuizInfo([FromRoute] Guid quizId)
        {
            try
            {
                var info = await _participantQuizService.GetQuizInfoAsync(quizId);
                return Ok(info);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpPost("{quizId}/start")]
        public async Task<ActionResult<StartQuizResponseDTO>> StartQuiz(
            [FromRoute] Guid quizId,
            [FromBody] StartQuizRequestDTO dto)
        {
            try
            {
                Guid? userIdClaim = null;
                try
                {
                    userIdClaim = GetCurrentUserId();
                }
                catch
                {
                    userIdClaim = null;
                }

                var result = await _participantQuizService.StartQuizAsync(quizId, dto, userIdClaim);
                return Ok(result);
            }
            catch (QuizTimeOverException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, new { Message = ex.Message, ParticipationId = ex.ParticipationId });
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

        [HttpPost("{quizId}/resume")]
        public async Task<ActionResult<StartQuizResponseDTO>> ResumeQuiz(
            [FromRoute] Guid quizId,
            [FromBody] StartQuizRequestDTO dto)
        {
            try
            {
                Guid? userIdClaim = null;
                try
                {
                    userIdClaim = GetCurrentUserId();
                }
                catch
                {
                    userIdClaim = null;
                }

                var result = await _participantQuizService.ResumeQuizAsync(quizId, dto, userIdClaim);
                return Ok(result);
            }
            catch (QuizTimeOverException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, new { Message = ex.Message, ParticipationId = ex.ParticipationId });
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

        [HttpGet("participation/{participationId}/content")]
        public async Task<ActionResult<QuizContentDTO>> GetQuizContent([FromRoute] Guid participationId)
        {
            try
            {
                var content = await _participantQuizService.GetQuizContentAsync(participationId);
                return Ok(content);
            }
            catch (QuizTimeOverException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, new { Message = ex.Message, ParticipationId = ex.ParticipationId });
            }
            catch (InvalidDataException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpPost("participation/{participationId}/save-answer")]
        public async Task<ActionResult> SaveAnswer(
            [FromRoute] Guid participationId,
            [FromBody] AnswerSubmissionDTO dto)
        {
            try
            {
                await _participantQuizService.SaveAnswerAsync(participationId, dto.QuestionId, dto.SelectedAnswerId);
                return Ok(new { Message = "Answer saved successfully" });
            }
            catch (QuizTimeOverException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, new { Message = ex.Message, ParticipationId = ex.ParticipationId });
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

        [HttpPost("participation/{participationId}/save-answers")]
        public async Task<ActionResult> SaveAnswers(
            [FromRoute] Guid participationId,
            [FromBody] List<AnswerSubmissionDTO> answers)
        {
            try
            {
                await _participantQuizService.SaveAnswersAsync(participationId, answers);
                return Ok(new { Message = "Answers saved successfully" });
            }
            catch (QuizTimeOverException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, new { Message = ex.Message, ParticipationId = ex.ParticipationId });
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

        [HttpPost("participation/{participationId}/submit")]
        public async Task<ActionResult<SubmitQuizResponseDTO>> SubmitQuiz(
            [FromRoute] Guid participationId)
        {
            try
            {
                // No body needed - answers already saved via save-answer API
                var result = await _participantQuizService.SubmitQuizAsync(participationId, new SubmitQuizDTO());
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

        [HttpGet("participation/{participationId}/result")]
        public async Task<ActionResult<SubmitQuizResponseDTO>> GetParticipationResult(
            [FromRoute] Guid participationId)
        {
            try
            {
                var result = await _participantQuizService.GetParticipationResultAsync(participationId);
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
    }
}
