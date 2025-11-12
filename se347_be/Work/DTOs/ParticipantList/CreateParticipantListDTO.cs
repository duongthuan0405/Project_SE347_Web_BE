using System.ComponentModel.DataAnnotations;

namespace se347_be.Work.DTOs.ParticipantList
{
    public class CreateParticipantListDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string Name { get; set; } = null!;
    }
}
