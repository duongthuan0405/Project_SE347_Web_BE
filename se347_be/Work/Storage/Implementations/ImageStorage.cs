using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using se347_be.Work.Storage;
using se347_be.Work.Storage.Interfaces;

namespace se347_be.Work.Storage.Implementations
{
    public class ImageStorage : IImageStorage
    {
        private readonly string _storagePath;
        private readonly string _subFolderByType = "images";
        private readonly ILogger<DocumentStorage> _logger;

        public ImageStorage(ILogger<DocumentStorage> logger, IConfiguration config, IWebHostEnvironment env)
        {
            var fileSettings = config.GetSection("FileSettings");
            var storagePath = fileSettings?["StoragePath"] ?? "wwwroot/uploads_df";

            if (!Path.IsPathRooted(storagePath))
            {
                _storagePath = Path.Combine(env.ContentRootPath, storagePath);
            }
            else
            {
                _storagePath = storagePath;
            }

            _logger = logger;

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
                Console.WriteLine("Create directory: " + _storagePath);
            }
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

        public bool Delete(string urlToFile)
        {
            if (!Path.IsPathRooted(urlToFile))
            {
                urlToFile = Path.Combine(_storagePath, urlToFile);
            }

            if (File.Exists(urlToFile))
            {
                File.Delete(urlToFile);
                return true;
            }
            return false;
        }
    }
}
