namespace se347_be.Work.DTOs.Answer
{
    public class AnswerResponseDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = "";
        public bool IsCorrectAnswer { get; set; }
    }
}
