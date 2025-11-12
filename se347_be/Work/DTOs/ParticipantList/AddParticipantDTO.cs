using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.ParticipantList
{
    public class AddParticipantDTO
    {
        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [StringLength(100)]
        public string? StudentId { get; set; }
    }
}
