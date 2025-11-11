namespace se347_be.Work.DTOs.Participant
{
    public class QuizContentDTO
    {
        public List<QuestionContentDTO> Questions { get; set; } = new();
        public Guid ParticipationId { get; set; }
        public int? DurationInMinutes { get; set; }
    }

    public class QuestionContentDTO
    {
        public Guid QuestionId { get; set; }
        public string Content { get; set; } = null!;
        public List<AnswerOptionDTO> Answers { get; set; } = new();
        public int OrderNumber { get; set; }
    }

    public class AnswerOptionDTO
    {
        public Guid AnswerId { get; set; }
        public string Content { get; set; } = null!;
        public string OptionLabel { get; set; } = null!; // A, B, C, D
    }
}
