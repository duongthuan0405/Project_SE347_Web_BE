using Microsoft.Extensions.Options;
using se347_be.Work.Controllers;
using se347_be.Work.Services.Interfaces;
using se347_be.Work.Storage;
using Sprache;

namespace se347_be.Work.Services.Implementations
{
    public class DocumentStorage : IDocumentStorage
    {
        private readonly string _storagePath;
        private readonly string _subFolderByType = "documents";
        private readonly ILogger<DocumentStorage> _logger;

        public DocumentStorage(ILogger<DocumentStorage> logger, IOptions<FileSettings> fileSettings, IWebHostEnvironment env) 
        {

            if (!Path.IsPathRooted(fileSettings.Value.StoragePath)) {
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

        public bool DeleteAsync(string urlToFile)
        {
            if(!Path.IsPathRooted(urlToFile))
            {
                urlToFile = Path.Combine(_storagePath, urlToFile);
            }

            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Delete document: " + urlToFile);
                Console.ResetColor();
            }


            if (System.IO.File.Exists(urlToFile))
            {
                System.IO.File.Delete(urlToFile);
                return true;
            }
            return false;
        }

        public async Task<string> SaveAsync(IFormFile file, string subFolder, string name)
        {
            try
            {
                var folderPath = Path.Combine(_storagePath, _subFolderByType, subFolder);

                if (!Directory.Exists(folderPath))
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
                return Path.Combine(_subFolderByType, subFolder, uniqueFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                throw;
            }
        }
    }
}
