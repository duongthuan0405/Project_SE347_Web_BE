using se347_be.Work.DTOs.Question;

namespace se347_be.Work.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<QuestionResponseDTO> CreateQuestionAsync(Guid quizId, CreateQuestionDTO createQuestionDTO, Guid creatorId);
        Task<List<QuestionResponseDTO>> GetQuestionsByQuizIdAsync(Guid quizId, Guid creatorId);
        Task<QuestionResponseDTO> UpdateQuestionAsync(Guid quizId, Guid questionId, UpdateQuestionDTO updateQuestionDTO, Guid creatorId);
        Task DeleteQuestionAsync(Guid quizId, Guid questionId, Guid creatorId);
    }
}
