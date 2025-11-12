using System.Text.RegularExpressions;
using ClosedXML.Excel;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs.Invite;
using se347_be.Work.Email;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class InviteService : IInviteService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IParticipantListRepository _participantListRepository;
        private readonly MyAppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public InviteService(
            IQuizRepository quizRepository,
            IParticipantListRepository participantListRepository,
            MyAppDbContext context,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _quizRepository = quizRepository;
            _participantListRepository = participantListRepository;
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
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

            // Send emails and save to whitelist (for Private access type)
            var response = new InviteResponseDTO();
            foreach (var participant in participants)
            {
                try
                {
                    await _emailService.SendInviteEmailAsync(participant, quiz.Title, quizLink, quiz.StartTime, quiz.DueTime);
                    
                    // Add to whitelist for Private quizzes
                    if (quiz.AccessType == "Private")
                    {
                        var invitation = new QuizInvitation
                        {
                            Id = Guid.NewGuid(),
                            QuizId = quizId,
                            Email = participant.Email,
                            StudentId = participant.StudentId,
                            FullName = participant.FullName,
                            InvitedAt = DateTime.Now
                        };
                        _context.QuizInvitations.Add(invitation);
                    }
                    
                    response.TotalSent++;
                }
                catch (Exception ex)
                {
                    response.Failed.Add($"{participant.Email}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
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

        

        public async Task<InviteResponseDTO> SendInvitesFromListsAsync(Guid quizId, List<Guid> participantListIds, Guid creatorId)
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

            // Collect all participants from the lists
            var allParticipants = new List<ParticipantInfoDTO>();
            
            foreach (var listId in participantListIds)
            {
                var participantList = await _participantListRepository.GetByIdWithParticipantsAsync(listId);
                
                if (participantList == null)
                {
                    continue; // Skip non-existent lists
                }

                // Verify list ownership
                if (participantList.CreatorId != creatorId)
                {
                    continue; // Skip lists not owned by creator
                }

                // Convert participants to DTO
                if (participantList.Participants != null)
                {
                    foreach (var participant in participantList.Participants)
                    {
                        // Avoid duplicate emails
                        if (!allParticipants.Any(p => p.Email == participant.Email))
                        {
                            allParticipants.Add(new ParticipantInfoDTO
                            {
                                FullName = participant.FullName,
                                Email = participant.Email,
                                StudentId = participant.StudentId ?? "",
                                ClassName = ""
                            });
                        }
                    }
                }
            }

            if (!allParticipants.Any())
            {
                throw new InvalidDataException("No valid participants found in the selected lists");
            }

            // Generate quiz link
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
            var quizLink = $"{baseUrl}/quiz/{quizId}";

            // Send emails and save to whitelist
            var response = new InviteResponseDTO();
            foreach (var participant in allParticipants)
            {
                try
                {
                    await _emailService.SendInviteEmailAsync(participant, quiz.Title, quizLink, quiz.StartTime, quiz.DueTime);
                    
                    // Add to whitelist for Private quizzes
                    if (quiz.AccessType == "Private")
                    {
                        var invitation = new QuizInvitation
                        {
                            Id = Guid.NewGuid(),
                            QuizId = quizId,
                            Email = participant.Email,
                            StudentId = participant.StudentId,
                            FullName = participant.FullName,
                            InvitedAt = DateTime.Now
                        };
                        _context.QuizInvitations.Add(invitation);
                    }
                    
                    response.TotalSent++;
                }
                catch (Exception ex)
                {
                    response.Failed.Add($"{participant.Email}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            return response;
        }
    }
}
