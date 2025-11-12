using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.Participant
{
    public class SubmitQuizDTO
    {
        // No body needed - answers already saved via save-answer API
        // This DTO kept for future extensions (e.g., submit timestamp validation)
    }

    public class AnswerSubmissionDTO
    {
        [Required]
        public Guid QuestionId { get; set; }

        [Required]
        public Guid SelectedAnswerId { get; set; }
    }

    public class SubmitQuizResponseDTO
    {
        public Guid ParticipationId { get; set; }
        public DateTime SubmitTime { get; set; }
        public decimal? Score { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public bool ShowScore { get; set; }
        public string Message { get; set; } = null!;
    }
}
