
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using se347_be.Work.DTOs.Invite;
using se347_be.Work.Repositories.Interfaces;

namespace se347_be.Work.Email
{
    public class EmailService : IEmailService
    {
        private readonly IPendingUserRepository _pendingUserRepository;
        private readonly string _username;
        private readonly string _password;
        private readonly string _server;
        private readonly int _port;
        private readonly string _displayName;

        public EmailService(IConfiguration config, IPendingUserRepository oTPVerifyEmailRepository)
        {
            _pendingUserRepository = oTPVerifyEmailRepository;
            var emailSettings = config.GetSection("EmailSettings");
            _username = emailSettings?["Username"] ?? "";
            _password = emailSettings?["Password"] ?? "";
            _server = emailSettings?["Server"] ?? "smtp.gmail.com";
            _displayName = emailSettings?["DisplayName"] ?? "SE347 - MyQuiz";

            var strPort = emailSettings?["Port"] ?? "587";
            int.TryParse(strPort, out _port);
        }

        private async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_displayName, _username));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder()
            {
                HtmlBody = htmlBody
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {

                await client.ConnectAsync(_server, _port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_username, _password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendOTPAsync(string to, bool isResend)
        {
            string? otp = await _pendingUserRepository.GenerateOTPForAsync(to, isResend);
            if (otp == null)
            {
                return;
            }
              

            var subject = "OTP FOR YOUR EMAIL VERIFICATION";
            var htmlBody = $@"<p>Your OTP is: <b>{otp}</b><p>";

            using (var client = new SmtpClient())
            {
                try
                {
                    await SendEmailAsync(to, subject, htmlBody);
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
            message.From.Add(new MailboxAddress(_displayName, _username));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = $"Quiz Result: {quizTitle}";

            var subject = $"Quiz Result: {quizTitle}";
            var htmlBody = $@"
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
                ";


            using (var client = new SmtpClient())
            {
                try
                {
                    await SendEmailAsync(to, subject, htmlBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email Sending Error: {ex.Message}");
                    throw new Exception("Failed to send quiz result email!");
                }
            }
        }

        public async Task SendInviteEmailAsync(
            ParticipantInfoDTO participant,
            string quizTitle,
            string quizLink,
            DateTime? startTime,
            DateTime? dueTime)
        {
            var timeInfo = "";
            if (startTime.HasValue && dueTime.HasValue)
            {
                timeInfo = $"<p><strong>Time:</strong> {startTime.Value:dd/MM/yyyy HH:mm} - {dueTime.Value:dd/MM/yyyy HH:mm}</p>";
            }

            var emailBody = $@"
                <html>
                <body>
                    <h2>Invitation to join the quiz: {quizTitle}</h2>
                    <p>Dear <strong>{participant.FullName}</strong>,</p>
                    <p>You have been invited to join the quiz</p>
                    {timeInfo}
                    <p><strong>Link to join:</strong></p>
                    <p><a href='{quizLink}' style='display:inline-block;padding:10px 20px;background-color:#007bff;color:white;text-decoration:none;border-radius:5px;'>Join now</a></p>
                    <p>Or copy the link: <a href='{quizLink}'>{quizLink}</a></p>
                    <br/>
                    <p>Good Luck!</p>
                    <p><em>MyQuizz</em></p>
                </body>
                </html>
            ";

            var subject = $"Invitation to join the quiz: {quizTitle}";

            using (var client = new SmtpClient())
            {
                try
                {
                    await SendEmailAsync(participant.Email, subject, emailBody);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to send email to {participant.Email}: {ex.Message}");
                }
            }
        }
    }
}
