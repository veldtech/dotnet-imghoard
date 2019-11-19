using Newtonsoft.Json;

namespace Imghoard.Models
{
    public class UploadResponse
    {
        [JsonProperty("File")]
        public string File { get; internal set; }
    }
}
