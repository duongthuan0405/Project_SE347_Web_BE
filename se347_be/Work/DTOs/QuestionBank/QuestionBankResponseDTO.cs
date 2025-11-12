namespace se347_be.Work.DTOs.QuestionBank
{
    public class QuestionBankResponseDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = null!;
        public int Points { get; set; }
        public string? Category { get; set; }
        public bool IsDraft { get; set; }
        public int AnswerCount { get; set; }
        public int UsedInQuizCount { get; set; } // How many quizzes use this question
    }

    public class QuestionBankDetailDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = null!;
        public int Points { get; set; }
        public string? Category { get; set; }
        public bool IsDraft { get; set; }
        public List<AnswerDetailDTO> Answers { get; set; } = new();
        public int UsedInQuizCount { get; set; }
    }

    public class AnswerDetailDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = null!;
        public bool IsCorrectAnswer { get; set; }
    }
}
