using se347_be.Work.Storage.Interfaces;

namespace se347_be.Work.Storage.Implementations
{
    public class ImageProcessor : IImageProcessor
    {
        public bool IsSupportedFileType(string fileName)
        {
            var allowedExtensions = new[] { ".jpg", ".png" };
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return allowedExtensions.Contains(extension);
        }
    }
}
