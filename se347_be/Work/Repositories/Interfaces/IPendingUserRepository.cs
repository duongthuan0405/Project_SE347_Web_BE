using se347_be.Work.Database.Entity;

namespace se347_be.Work.Repositories.Interfaces
{
    public interface IPendingUserRepository
    {
        public Task<string> AddNewPendingUserAsync(PendingUser pendingUser);
        Task<string?> GenerateOTPForAsync(string to, bool isReGenerate = false);
        Task RemovePendingUserAsync(string to);
        Task<PendingUser> VerifyAccountAsync(string email, string otp);
    }
}
