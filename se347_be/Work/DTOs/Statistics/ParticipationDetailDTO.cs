namespace se347_be.Work.DTOs.Statistics
{
    public class ParticipationDetailDTO
    {
        public Guid ParticipationId { get; set; }
        public string? FullName { get; set; }
        public string? StudentId { get; set; }
        public string? ClassName { get; set; }
        public double Score { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public DateTime? SubmitTime { get; set; }
        public List<QuestionResultDTO> Details { get; set; } = new();
    }

    public class QuestionResultDTO
    {
        public string QuestionContent { get; set; } = "";
        public string? SelectedAnswer { get; set; }
        public string CorrectAnswer { get; set; } = "";
        public bool IsCorrect { get; set; }
    }
}
