using Microsoft.Extensions.Logging;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class ImageStorage : IImageStorage
    {
        private readonly string _storagePath;  
        private readonly ILogger<ImageStorage> _logger;
        public ImageStorage(IConfiguration configuration, ILogger<ImageStorage> logger)
        {
            var path = configuration["FileUpload:Path"] ?? Path.Combine("wwwroot", "uploads");
            Console.WriteLine(Directory.GetCurrentDirectory());
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), path, "avatars");


            _logger = logger;

            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);
        }

        public async Task<string> SaveAsync(IFormFile file, string subFolder, string name)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is null or empty");

            // Thư mục con
            var folderPath = string.IsNullOrWhiteSpace(subFolder)
                ? _storagePath
                : Path.Combine(_storagePath, subFolder);

            if (Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Tạo tên file duy nhất
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = !string.IsNullOrEmpty(name) ? $"{name}{extension}" : $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

           
            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Image saved: {FilePath}", filePath);

            return filePath;
        }

        Task<bool> IFileStorage.DeleteAsync(string urlToFile)
        {
            throw new NotImplementedException();
        }
    }
}
