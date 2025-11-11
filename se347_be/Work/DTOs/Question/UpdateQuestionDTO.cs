using System.ComponentModel.DataAnnotations;
using se347_be.Work.DTOs.Answer;

namespace se347_be.Work.DTOs.Question
{
    public class UpdateQuestionDTO
    {
        [StringLength(255)]
        public string? Content { get; set; }

        public List<CreateAnswerDTO>? Answers { get; set; }
    }
}
