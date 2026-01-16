using se347_be.Work.DTOs.QuestionBank;

namespace se347_be.Work.Services.Interfaces
{
    public interface IQuestionBankService
    {
        Task<List<QuestionBankDetailDTO>> CreateQuestionsAsync(List<CreateQuestionBankDTO> dtos, Guid creatorId);
        Task<List<QuestionBankResponseDTO>> GetQuestionsAsync(Guid creatorId, string? category = null, string? searchTerm = null);
        Task<QuestionBankDetailDTO> GetQuestionDetailAsync(Guid questionId, Guid creatorId);
        Task<QuestionBankDetailDTO> UpdateQuestionAsync(Guid questionId, UpdateQuestionBankDTO dto, Guid creatorId);
        Task DeleteQuestionAsync(Guid questionId, Guid creatorId);
    }
}
