namespace se347_be.Work.DTOs.Quiz
{
    public class GenerateQuizLinkDTO
    {
        public Guid QuizId { get; set; }
        public string PublicLink { get; set; } = null!;
        public string? AccessCode { get; set; }
        public bool RequiresAccessCode { get; set; }
    }

    public class GenerateAccessCodeDTO
    {
        public string AccessCode { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
