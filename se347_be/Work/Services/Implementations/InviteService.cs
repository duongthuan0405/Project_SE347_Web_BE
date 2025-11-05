using System.Text.RegularExpressions;
using ClosedXML.Excel;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using se347_be.Email;
using se347_be.Work.DTOs.Invite;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class InviteService : IInviteService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly EmailSettings _emailSettings;
        private readonly IConfiguration _configuration;

        public InviteService(
            IQuizRepository quizRepository,
            IOptions<EmailSettings> emailSettings,
            IConfiguration configuration)
        {
            _quizRepository = quizRepository;
            _emailSettings = emailSettings.Value;
            _configuration = configuration;
        }

        public async Task<InviteResponseDTO> SendInvitesAsync(Guid quizId, IFormFile excelFile, Guid creatorId)
        {
            // Verify quiz ownership
            var isOwned = await _quizRepository.IsQuizOwnedByUserAsync(quizId, creatorId);
            if (!isOwned)
            {
                throw new UnauthorizedAccessException("You don't have permission to send invites for this quiz");
            }

            // Get quiz details
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            if (quiz == null)
            {
                throw new InvalidDataException("Quiz not found");
            }

            // Check if quiz is published
            if (!quiz.IsPublish)
            {
                throw new InvalidDataException("Cannot send invites for unpublished quiz");
            }

            // Parse Excel file
            var participants = await ParseExcelFileAsync(excelFile);

            if (!participants.Any())
            {
                throw new InvalidDataException("No valid participants found in Excel file");
            }

            // Generate quiz link
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
            var quizLink = $"{baseUrl}/quiz/{quizId}";

            // Send emails
            var response = new InviteResponseDTO();
            foreach (var participant in participants)
            {
                try
                {
                    await SendInviteEmailAsync(participant, quiz.Title, quizLink, quiz.StartTime, quiz.DueTime);
                    response.TotalSent++;
                }
                catch (Exception ex)
                {
                    response.Failed.Add($"{participant.Email}: {ex.Message}");
                }
            }

            return response;
        }

        private async Task<List<ParticipantInfoDTO>> ParseExcelFileAsync(IFormFile file)
        {
            var participants = new List<ParticipantInfoDTO>();

            if (file == null || file.Length == 0)
            {
                throw new InvalidDataException("File is empty");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
            {
                throw new InvalidDataException("Only Excel files (.xlsx, .xls) are supported");
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            // Find header row (assume first row)
            var headerRow = worksheet.Row(1);
            var fullNameCol = FindColumn(headerRow, "FullName", "Name", "Họ Tên", "HoTen");
            var emailCol = FindColumn(headerRow, "Email", "E-mail");
            var studentIdCol = FindColumn(headerRow, "StudentId", "MSSV", "StudentID", "Student ID");
            var classNameCol = FindColumn(headerRow, "ClassName", "Class", "Lớp", "Lop");

            if (fullNameCol == 0 || emailCol == 0)
            {
                throw new InvalidDataException("Excel file must contain 'FullName' and 'Email' columns");
            }

            // Read data rows
            var rows = worksheet.RowsUsed().Skip(1); // Skip header
            foreach (var row in rows)
            {
                var email = row.Cell(emailCol).GetString().Trim();
                
                // Validate email
                if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
                {
                    continue; // Skip invalid rows
                }

                participants.Add(new ParticipantInfoDTO
                {
                    FullName = row.Cell(fullNameCol).GetString().Trim(),
                    Email = email,
                    StudentId = studentIdCol > 0 ? row.Cell(studentIdCol).GetString().Trim() : "",
                    ClassName = classNameCol > 0 ? row.Cell(classNameCol).GetString().Trim() : ""
                });
            }

            return participants;
        }

        private int FindColumn(IXLRow headerRow, params string[] possibleNames)
        {
            for (int col = 1; col <= headerRow.CellsUsed().Count(); col++)
            {
                var cellValue = headerRow.Cell(col).GetString().Trim();
                if (possibleNames.Any(name => cellValue.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return col;
                }
            }
            return 0;
        }

        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        private async Task SendInviteEmailAsync(
            ParticipantInfoDTO participant,
            string quizTitle,
            string quizLink,
            DateTime? startTime,
            DateTime? dueTime)
        {
            var timeInfo = "";
            if (startTime.HasValue && dueTime.HasValue)
            {
                timeInfo = $"<p><strong>Thời gian:</strong> {startTime.Value:dd/MM/yyyy HH:mm} - {dueTime.Value:dd/MM/yyyy HH:mm}</p>";
            }

            var emailBody = $@"
                <html>
                <body>
                    <h2>Lời mời tham gia bài thi: {quizTitle}</h2>
                    <p>Xin chào <strong>{participant.FullName}</strong>,</p>
                    <p>Bạn được mời tham gia bài thi trắc nghiệm trực tuyến.</p>
                    {timeInfo}
                    <p><strong>Link làm bài:</strong></p>
                    <p><a href='{quizLink}' style='display:inline-block;padding:10px 20px;background-color:#007bff;color:white;text-decoration:none;border-radius:5px;'>Bắt đầu làm bài</a></p>
                    <p>Hoặc copy link sau: <a href='{quizLink}'>{quizLink}</a></p>
                    <br/>
                    <p>Chúc bạn làm bài tốt!</p>
                    <p><em>MyQuizz - Hệ thống thi trắc nghiệm trực tuyến</em></p>
                </body>
                </html>
            ";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("MyQuizz", _emailSettings.Username));
            message.To.Add(MailboxAddress.Parse(participant.Email));
            message.Subject = $"Lời mời tham gia bài thi: {quizTitle}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = emailBody
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_emailSettings.Server, _emailSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email to {participant.Email}: {ex.Message}");
            }
        }
    }
}
