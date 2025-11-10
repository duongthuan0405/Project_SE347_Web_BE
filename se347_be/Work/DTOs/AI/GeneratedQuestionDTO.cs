namespace se347_be.Work.DTOs.AI
{
    public class GeneratedQuestionDTO
    {
        public string Question { get; set; } = null!;
        public List<GeneratedAnswerDTO> Answers { get; set; } = new();
        public int Points { get; set; } = 1;
    }

    public class GeneratedAnswerDTO
    {
        public string Content { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }

    public class GenerateQuizResponseDTO
    {
        public List<GeneratedQuestionDTO> Questions { get; set; } = new();
        public string SourceFileName { get; set; } = null!;
        public int GeneratedCount { get; set; }
    }

    public class GenerateAndSaveQuizResponseDTO
    {
        public List<Guid> QuestionIds { get; set; } = new(); // IDs of created questions
        public int SavedCount { get; set; }
        public string SourceFileName { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
