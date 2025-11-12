using se347_be.Work.DTOs.Invite;

namespace se347_be.Work.Email
{
    public interface IEmailService
    {
        public Task SendOTPAsync(string to, bool isResend = false);
        public Task SendQuizResultEmailAsync(string to, string participantName, string quizTitle, decimal score, int correctAnswers, int totalQuestions);
        public Task SendInviteEmailAsync(ParticipantInfoDTO participant, string quizTitle, string quizLink, DateTime? startTime, DateTime? dueTime);
    }
}
