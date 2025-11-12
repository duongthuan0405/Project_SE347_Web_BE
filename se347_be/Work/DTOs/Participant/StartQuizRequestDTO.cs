using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.Participant
{
    public class StartQuizRequestDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        public string? StudentId { get; set; }
        
        public string? ClassName { get; set; }

        public string? AccessCode { get; set; }
    }

    public class StartQuizResponseDTO
    {
        public Guid ParticipationId { get; set; }
        public Guid QuizId { get; set; }
        public string QuizTitle { get; set; } = null!;
        public string? Description { get; set; }
        public int? DurationInMinutes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EstimatedEndTime { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsShuffleQuestions { get; set; }
        public bool IsShuffleAnswers { get; set; }
    }
}
