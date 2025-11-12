using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.Quiz
{
    public class GenerateQuestionsFromDocumentDTO
    {
        [Required(ErrorMessage = "Document ID is required")]
        public Guid DocumentId { get; set; }

        [Range(1, 50, ErrorMessage = "Number of questions must be between 1 and 50")]
        public int NumberOfQuestions { get; set; } = 10;

        [StringLength(500)]
        public string? AdditionalInstructions { get; set; }
    }
}
