using Microsoft.Extensions.Options;
using se347_be.Work.Storage;

namespace se347_be.Work.URLFileHelper
{
    public class URLHelper : IURLHelper
    {
        private readonly string _baseURL = "";
        public URLHelper(IOptions<FileSettings> fileSettings, IConfiguration config)
        {
            _baseURL = config["AppSettings:BaseURL"] ?? "http://localhost:5007";

            var requestPath = fileSettings.Value.RequestPath;
            _baseURL = new Uri(new Uri(_baseURL), requestPath + "/").ToString();
            
        }

        public string GetLiveURL(string relativeURL)
        {

            string url = new Uri(new Uri(_baseURL), relativeURL.Replace("\\", "/")).ToString();

            return url;
        }

       
    }
}
