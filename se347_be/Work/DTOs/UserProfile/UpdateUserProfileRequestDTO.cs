namespace se347_be.Work.DTOs.UserProfile
{
    public class UpdateUserProfileRequestDTO
    {
        public string? LastName { get; set; } = null!;
        public string? FirstName { get; set; } = null!;
        public IFormFile? ImageFile { get; set; } = null!;
    }
}
