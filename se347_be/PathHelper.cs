namespace se347_be
{
    public static class PathHelper
    {
        public static string GetPhysicalPath(string relativePath, string uploadPath)
        {
            return Path.Combine(uploadPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        // Lấy URL public từ đường dẫn tương đối lưu trong DB
        public static string GetUrl(string relativePath, string requestPath = "/uploads")
        {
            relativePath = relativePath.Replace("\\", "/"); // đảm bảo URL chuẩn
            return $"{requestPath}/{relativePath}";
        }
    }
}
