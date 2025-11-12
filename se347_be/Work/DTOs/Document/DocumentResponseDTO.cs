namespace se347_be.Work.DTOs.Document
{
    public class DocumentResponseDTO
    {
        public Guid Id { get; set; }
        public Guid QuizId { get; set; }
        public string FileName { get; set; } = null!;
        public string StorageUrl { get; set; } = null!;
        public string Status { get; set; } = "Uploaded";
        public DateTime? UploadAt { get; set; }
    }
}
