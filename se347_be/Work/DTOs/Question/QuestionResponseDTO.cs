using se347_be.Work.DTOs.Answer;

namespace se347_be.Work.DTOs.Question
{
    public class QuestionResponseDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = "";
        public Guid? QuizId { get; set; }
        public List<AnswerResponseDTO>? Answers { get; set; }
        public int Score { get; set; } = 1;
    }
}
