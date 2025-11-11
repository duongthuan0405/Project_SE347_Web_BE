using se347_be.Work.Controllers;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class DocumentStorage : IDocumentStorage
    {
        private readonly string _storagePath;
        private readonly ILogger<DocumentStorage> _logger;

        public DocumentStorage(ILogger<DocumentStorage> logger, IConfiguration configuration) 
        {
            var path = configuration["FileUpload:Path"]/* ?? Path.Combine("wwwroot", "uploads")*/;
            Console.WriteLine(Directory.GetCurrentDirectory());
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), path, "documents");

            _logger = logger;

            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);
        }

        public async Task<bool> DeleteAsync(string urlToFile)
        {
            if (System.IO.File.Exists(urlToFile))
            {
                Console.WriteLine("Có file");
                System.IO.File.Delete(urlToFile);
                return true;
            }
            return false;
        }

        public async Task<string> SaveAsync(IFormFile file, string subFolder, string name)
        {
            try
            {
                var folderPath = string.IsNullOrWhiteSpace(subFolder)
                ? _storagePath
                : Path.Combine(_storagePath, subFolder);

                if (Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = !string.IsNullOrEmpty(name) ? $"{name}{fileExtension}" : $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(folderPath, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File uploaded: {FilePath}", filePath);
                return "http://localhost:5007/uploads/documents/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                throw;
            }
        }
    }
}
