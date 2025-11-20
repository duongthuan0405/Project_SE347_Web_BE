using Microsoft.Extensions.Options;
using se347_be.Work.Storage;

namespace se347_be.Work.URLFileHelper
{
    public class URLHelper : IURLHelper
    {
        private readonly string _baseURL = "";
        public URLHelper(IConfiguration config)
        {
            _baseURL = config["AppSettings:BaseUrl"] ?? "http://localhost:5007";

            var requestPath = config["FileSettings:RequestPath"] ?? "uploads_df";
            _baseURL = new Uri(new Uri(_baseURL), requestPath + "/").ToString();
            
        }

        public string GetLiveURL(string relativeURL)
        {

            string url = new Uri(new Uri(_baseURL), relativeURL).ToString();
            return url;
        }

       
    }
}
