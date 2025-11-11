using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.QuestionBank
{
    public class UpdateQuestionBankDTO
    {
        [StringLength(500)]
        public string? Content { get; set; }

        [Range(1, 100)]
        public int? Points { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        public List<UpdateAnswerDTO>? Answers { get; set; }
    }

    public class UpdateAnswerDTO
    {
        public Guid? Id { get; set; } // If null, create new answer

        [StringLength(500)]
        public string? Content { get; set; }

        public bool? IsCorrectAnswer { get; set; }

        public bool IsDeleted { get; set; } = false; // Mark for deletion
    }
}
