using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.ParticipantList
{
    public class ImportParticipantsResponseDTO
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
