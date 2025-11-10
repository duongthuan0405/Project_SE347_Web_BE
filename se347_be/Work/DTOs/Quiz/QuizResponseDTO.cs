namespace se347_be.Work.DTOs.Quiz
{
    public class QuizResponseDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? DueTime { get; set; }
        public int MaxTimesCanAttempt { get; set; }
        public bool IsPublish { get; set; }
        public bool IsShuffleAnswers { get; set; }
        public bool IsShuffleQuestions { get; set; }
        public int? DurationInMinutes { get; set; }
        public string? AccessCode { get; set; }
        public string AccessType { get; set; } = "Public";
        public bool ShowScoreAfterSubmission { get; set; }
        public bool SendResultEmail { get; set; }
        public Guid? CreatorId { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalParticipants { get; set; }
    }
}
