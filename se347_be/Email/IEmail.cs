namespace se347_be.Email
{
    public interface IEmail
    {
        public Task SendOTPAsync(string to, bool isResend = false);
    }
}
