namespace se347_be.Work.Email
{
    public interface IEmail
    {
        public Task SendOTPAsync(string to, bool isResend = false);
        public Task SendQuizResultEmailAsync(string to, string participantName, string quizTitle, decimal score, int correctAnswers, int totalQuestions);
    }
}
