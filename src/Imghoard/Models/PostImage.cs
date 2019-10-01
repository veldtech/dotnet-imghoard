using System.IO;
using System.Text.Json.Serialization;
using Imghoard.Formatters;

namespace Imghoard.Models
{
    internal struct PostImage
    {
        [JsonPropertyName("Tags")]
        public string[] Tags { get; internal set; }

        [JsonPropertyName("Data")]
        [JsonConverter(typeof(Base64FileFormatter))]
        public Stream Stream { get; internal set; }
    }
}
