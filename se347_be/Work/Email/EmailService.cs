
using Microsoft.Extensions.Options;
using se347_be.Work.Repositories.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;

namespace se347_be.Work.Email
{
    public class EmailService : IEmail
    {
        private readonly EmailSettings _emailSettings;
        private readonly IPendingUserRepository _pendingUserRepository;


        public EmailService(IOptions<EmailSettings> emailSettings, IPendingUserRepository oTPVerifyEmailRepository)
        {
            _pendingUserRepository = oTPVerifyEmailRepository;
            _emailSettings = emailSettings.Value;
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(_emailSettings.Username + "\n" + _emailSettings.Password + "\n" + _emailSettings.Server + "\n" + _emailSettings.Port);
                Console.ResetColor();
            }
        }

        public async Task SendOTPAsync(string to, bool isResend)
        {
            string? otp = await _pendingUserRepository.GenerateOTPForAsync(to, isResend);
            if (otp == null)
            {
                return;
            }
              

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("SE347", _emailSettings.Username));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = "OTP FOR YOUR EMAIL VERIFICATION";

            var bodyBuilder = new BodyBuilder()
            {
                HtmlBody = $"<p>Your OTP is: <b>{otp}</b><p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailSettings.Server, _emailSettings.Port, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email Sending Error: {ex.Message}");
                    await _pendingUserRepository.RemovePendingUserAsync(to);
                    throw new Exception("Verification Email was sent unsuccessfully!");
                }
                finally
                {
                    Console.WriteLine(otp);
                }
            }
        }

        public async Task SendQuizResultEmailAsync(string to, string participantName, string quizTitle, decimal score, int correctAnswers, int totalQuestions)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("MyQuizz System", _emailSettings.Username));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = $"Quiz Result: {quizTitle}";

            var bodyBuilder = new BodyBuilder()
            {
                HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2 style='color: #2c3e50;'>Quiz Result</h2>
                        <p>Hello <strong>{participantName}</strong>,</p>
                        <p>Thank you for completing the quiz: <strong>{quizTitle}</strong></p>
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='color: #27ae60; margin-top: 0;'>Your Score: {score:F2}%</h3>
                            <p><strong>Correct Answers:</strong> {correctAnswers} out of {totalQuestions}</p>
                        </div>
                        <p>Best regards,<br/>MyQuizz Team</p>
                    </body>
                    </html>
                "
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailSettings.Server, _emailSettings.Port, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email Sending Error: {ex.Message}");
                    throw new Exception("Failed to send quiz result email!");
                }
            }
        }
    }
}
