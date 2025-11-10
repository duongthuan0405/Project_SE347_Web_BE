namespace se347_be.Work.DTOs.Participant
{
    public class QuizInfoDTO
    {
        public Guid QuizId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? DueTime { get; set; }
        public int? DurationInMinutes { get; set; }
        public int TotalQuestions { get; set; }
        public bool RequiresAccessCode { get; set; }
        public bool IsAvailable { get; set; }
        public string? Message { get; set; }
        public string PresentationMode { get; set; } = "AllAtOnce"; // "AllAtOnce", "OneByOne"
        public bool AllowNavigationBack { get; set; } = true;
    }
}
