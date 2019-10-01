using System.Text.Json.Serialization;

namespace Imghoard.Models
{
    public class ImagePostResult
    {
        [JsonPropertyName("file")]
        public string File { get; set; }
    }
}