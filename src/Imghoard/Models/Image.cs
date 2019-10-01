using System.Text.Json.Serialization;

namespace Imghoard.Models
{
    public class Image
    {
        [JsonPropertyName("id")]
        public ulong Id { get; internal set; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; internal set; }

        [JsonPropertyName("url")]
        public string Url { get; internal set; }
    }
}
