using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using se347_be.Work.Services.Interfaces;
using se347_be.Work.Storage;

namespace se347_be.Work.Services.Implementations
{
    public class ImageStorage : IImageStorage
    {
        private readonly string _storagePath;
        private readonly string _subFolderByType = "images";
        private readonly ILogger<DocumentStorage> _logger;

        public ImageStorage(ILogger<DocumentStorage> logger, IOptions<FileSettings> fileSettings, IWebHostEnvironment env)
        {

            if (!Path.IsPathRooted(fileSettings.Value.StoragePath))
            {
                _storagePath = Path.Combine(env.ContentRootPath, fileSettings.Value.StoragePath);
            }
            else
            {
                _storagePath = Path.Combine(fileSettings.Value.StoragePath);
            }

            _logger = logger;

            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);
        }

        public async Task<string> SaveAsync(IFormFile file, string subFolder, string name)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is null or empty");

            // Thư mục con
            var folderPath = Path.Combine(_storagePath, _subFolderByType, subFolder);

            if (!Directory.Exists(folderPath))
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

            return Path.Combine(_subFolderByType, subFolder, uniqueFileName);
        }

        public bool DeleteAsync(string urlToFile)
        {
            if (!Path.IsPathRooted(urlToFile))
            {
                urlToFile = Path.Combine(_storagePath, urlToFile);
            }

            if (System.IO.File.Exists(urlToFile))
            {
                System.IO.File.Delete(urlToFile);
                return true;
            }
            return false;
        }
    }
}
