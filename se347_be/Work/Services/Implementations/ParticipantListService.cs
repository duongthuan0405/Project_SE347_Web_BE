using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs.ParticipantList;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;
using se347_be.Database;

namespace se347_be.Work.Services.Implementations
{
    public class ParticipantListService : IParticipantListService
    {
        private readonly IParticipantListRepository _listRepository;
        private readonly MyAppDbContext _context;

        public ParticipantListService(IParticipantListRepository listRepository, MyAppDbContext context)
        {
            _listRepository = listRepository;
            _context = context;
        }

        public async Task<ParticipantListResponseDTO> CreateListAsync(CreateParticipantListDTO dto, Guid creatorId)
        {
            var list = new ParticipantList
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                CreatorId = creatorId
            };

            await _listRepository.CreateAsync(list);

            return new ParticipantListResponseDTO
            {
                Id = list.Id,
                Name = list.Name,
                CreatorId = list.CreatorId,
                ParticipantCount = 0
            };
        }

        public async Task<List<ParticipantListResponseDTO>> GetListsByCreatorAsync(Guid creatorId)
        {
            var lists = await _listRepository.GetByCreatorIdAsync(creatorId);

            return lists.Select(l => new ParticipantListResponseDTO
            {
                Id = l.Id,
                Name = l.Name,
                CreatorId = l.CreatorId,
                ParticipantCount = l.Participants?.Count ?? 0
            }).ToList();
        }

        public async Task<ParticipantListDetailDTO> GetListDetailAsync(Guid listId, Guid userId)
        {
            var list = await _listRepository.GetByIdWithParticipantsAsync(listId);
            
            if (list == null)
                throw new InvalidDataException("Participant list not found");

            if (list.CreatorId != userId)
                throw new UnauthorizedAccessException("You do not have permission to access this list");

            return new ParticipantListDetailDTO
            {
                Id = list.Id,
                Name = list.Name,
                CreatorId = list.CreatorId,
                Participants = list.Participants?.Select(p => new ParticipantDTO
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    StudentId = p.StudentId,
                    Email = p.Email
                }).ToList() ?? new List<ParticipantDTO>()
            };
        }

        public async Task<ParticipantListResponseDTO> UpdateListAsync(Guid listId, UpdateParticipantListDTO dto, Guid userId)
        {
            var list = await _listRepository.GetByIdAsync(listId);
            
            if (list == null)
                throw new InvalidDataException("Participant list not found");

            if (list.CreatorId != userId)
                throw new UnauthorizedAccessException("You do not have permission to update this list");

            list.Name = dto.Name;
            await _listRepository.UpdateAsync(list);

            return new ParticipantListResponseDTO
            {
                Id = list.Id,
                Name = list.Name,
                CreatorId = list.CreatorId,
                ParticipantCount = list.Participants?.Count ?? 0
            };
        }

        public async Task DeleteListAsync(Guid listId, Guid userId)
        {
            var list = await _listRepository.GetByIdAsync(listId);
            
            if (list == null)
                throw new InvalidDataException("Participant list not found");

            if (list.CreatorId != userId)
                throw new UnauthorizedAccessException("You do not have permission to delete this list");

            await _listRepository.DeleteAsync(listId);
        }

        public async Task<ImportParticipantsResponseDTO> ImportParticipantsAsync(Guid listId, IFormFile file, Guid userId)
        {
            if (!await _listRepository.IsOwnerAsync(listId, userId))
                throw new UnauthorizedAccessException("You do not have permission to modify this list");

            var response = new ImportParticipantsResponseDTO();

            if (file == null || file.Length == 0)
                throw new InvalidDataException("File is empty or not provided");

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Only .xlsx files are supported");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // Skip header

            foreach (var row in rows)
            {
                try
                {
                    var fullName = row.Cell(1).GetValue<string>()?.Trim();
                    var studentId = row.Cell(2).GetValue<string>()?.Trim();
                    var email = row.Cell(3).GetValue<string>()?.Trim();

                    if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
                    {
                        response.FailureCount++;
                        response.Errors.Add($"Row {row.RowNumber()}: Missing required fields (FullName or Email)");
                        continue;
                    }

                    var participant = new Participant
                    {
                        Id = Guid.NewGuid(),
                        ListId = listId,
                        FullName = fullName,
                        StudentId = studentId,
                        Email = email
                    };

                    _context.Participants.Add(participant);
                    response.SuccessCount++;
                }
                catch (Exception ex)
                {
                    response.FailureCount++;
                    response.Errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            return response;
        }

        public async Task<ParticipantResponseDTO> AddParticipantAsync(Guid listId, AddParticipantDTO dto, Guid userId)
        {
            // Check ownership
            if (!await _listRepository.IsOwnerAsync(listId, userId))
                throw new UnauthorizedAccessException("You do not have permission to modify this list");

            // Check duplicate email
            var existingParticipant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ListId == listId && p.Email == dto.Email);

            if (existingParticipant != null)
                throw new InvalidDataException($"Participant with email {dto.Email} already exists in this list");

            // Create participant
            var participant = new Participant
            {
                Id = Guid.NewGuid(),
                ListId = listId,
                FullName = dto.FullName,
                Email = dto.Email,
                StudentId = dto.StudentId
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return new ParticipantResponseDTO
            {
                Id = participant.Id,
                FullName = participant.FullName,
                Email = participant.Email,
                StudentId = participant.StudentId
            };
        }
    }
}
