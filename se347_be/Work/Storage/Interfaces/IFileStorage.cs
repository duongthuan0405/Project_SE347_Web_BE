namespace se347_be.Work.Storage.Interfaces
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(IFormFile file, string subFolder = "", string name = "");
        bool Delete(string urlToFile);
        string GetFullPath(string urlToFile);
    }
}
