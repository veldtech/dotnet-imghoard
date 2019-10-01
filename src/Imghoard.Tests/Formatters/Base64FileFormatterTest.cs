using System.Text;
using System.Text.Json;
using Imghoard.Models;
using Xunit;

namespace Imghoard.Tests.Formatters
{
    public class Base64FileFormatterTest
    {
        [Fact]
        public void FormatTest()
        {
            using var pixel = typeof(Base64FileFormatterTest).Assembly
                .GetManifestResourceStream("Imghoard.Tests.Resources.BlackPixel.png");

            var bytes = JsonSerializer.SerializeToUtf8Bytes(
                new PostImage
                {
                    Stream = pixel
                },
                ImghoardClient.JsonSerializerOptions
            );

            Assert.Equal(
                @"{""Data"":""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAAAAAA6fptVAAAACklEQVR4nGNiAAAABgADNjd8qAAAAABJRU5ErkJggg==""}",
                Encoding.UTF8.GetString(bytes)
            );
        }
    }
}
