using Microsoft.Extensions.Options;
using se347_be.Work.Controllers;
using se347_be.Work.Storage;
using se347_be.Work.Storage.Interfaces;
using Sprache;

namespace se347_be.Work.Storage.Implementations
{
    public class DocumentStorage : IDocumentStorage
    {
        private readonly string _storagePath;
        private readonly string _subFolderByType = "documents";
        private readonly ILogger<DocumentStorage> _logger;

        public DocumentStorage(ILogger<DocumentStorage> logger, IConfiguration config, IWebHostEnvironment env)
        {

            var fileSettings = config.GetSection("FileSettings");
            var storagePath = fileSettings?["StoragePath"] ?? "wwwroot/uploads_df";

            if (!Path.IsPathRooted(storagePath)) {
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

        public bool Delete(string urlToFile)
        {
            if(!Path.IsPathRooted(urlToFile))
            {
                urlToFile = Path.Combine(_storagePath, urlToFile);
            }

            lock (Console.Out)
            {
                Console.WriteLine("Delete file: " + urlToFile);
            }


            if (File.Exists(urlToFile))
            {
                File.Delete(urlToFile);
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
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine("Create directory: " + folderPath);
                }

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
                Console.WriteLine(Path.Combine(_subFolderByType, subFolder, uniqueFileName));
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
