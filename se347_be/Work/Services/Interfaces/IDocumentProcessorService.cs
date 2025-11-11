using Microsoft.AspNetCore.Http;

namespace se347_be.Work.Services.Interfaces
{
    public interface IDocumentProcessorService
    {
        Task<string> ExtractTextFromFileAsync(IFormFile file);
        bool IsSupportedFileType(string fileName);
    }
}
