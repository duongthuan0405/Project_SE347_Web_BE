namespace se347_be.Work.Services.Interfaces
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(IFormFile file, string subFolder = "", string name = "");
        bool DeleteAsync(string urlToFile);
    }
}
