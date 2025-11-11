using Microsoft.AspNetCore.Http;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class DocumentProcessorService : IDocumentProcessorService
    {
        private readonly ILogger<DocumentProcessorService> _logger;
        private static readonly string[] SupportedExtensions = { ".txt", ".docx", ".pdf" };

        public DocumentProcessorService(ILogger<DocumentProcessorService> logger)
        {
            _logger = logger;
        }

        public bool IsSupportedFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return SupportedExtensions.Contains(extension);
        }

        public async Task<string> ExtractTextFromFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidDataException("File is empty or not provided");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            try
            {
                return extension switch
                {
                    ".txt" => await ExtractFromTxtAsync(file),
                    ".docx" => await ExtractFromDocxAsync(file),
                    ".pdf" => await ExtractFromPdfAsync(file),
                    _ => throw new InvalidDataException($"Unsupported file type: {extension}. Supported types: .txt, .docx, .pdf")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract text from file: {FileName}", file.FileName);
                throw new InvalidOperationException($"Failed to process file: {ex.Message}", ex);
            }
        }

        private async Task<string> ExtractFromTxtAsync(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        private async Task<string> ExtractFromDocxAsync(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var doc = WordprocessingDocument.Open(stream, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            
            if (body == null)
            {
                throw new InvalidDataException("Document body is empty");
            }

            var textBuilder = new StringBuilder();
            foreach (var paragraph in body.Elements<Paragraph>())
            {
                textBuilder.AppendLine(paragraph.InnerText);
            }

            return textBuilder.ToString();
        }

        private async Task<string> ExtractFromPdfAsync(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new PdfDocument(pdfReader);

            var textBuilder = new StringBuilder();
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                var text = PdfTextExtractor.GetTextFromPage(page, strategy);
                textBuilder.AppendLine(text);
            }

            return textBuilder.ToString();
        }
    }
}
