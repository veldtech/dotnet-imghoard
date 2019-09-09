using Newtonsoft.Json;

namespace Imghoard.Models
{
    public class Image
    {
        [JsonProperty("ID")]
        public ulong Id { get; internal set; }
        [JsonProperty("Tags")]
        public string[] Tags { get; internal set; }
        [JsonProperty("URL")]
        public string Url { get; internal set; }
    }
}
