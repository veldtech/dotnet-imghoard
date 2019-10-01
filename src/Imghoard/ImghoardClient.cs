using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Imghoard.Models;

namespace Imghoard
{
    public class ImghoardClient : IDisposable
    {
        internal static readonly MediaTypeHeaderValue JsonHeaderValue = new MediaTypeHeaderValue("application/json");
        internal static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true
        };

        private readonly HttpClient apiClient;

        public ImghoardClient(Config config)
        {
            apiClient = new HttpClient
            {
                BaseAddress = config.Endpoint,
                DefaultRequestHeaders =
                {
                    {"x-miki-tenancy", config.Tenancy}
                }
            };
        }

        public ImghoardClient(Uri endpoint)
            : this (new Config { Endpoint = endpoint })
        {
        }

        public ImghoardClient()
            : this(Config.Default())
        {
        }

        private static void ValidateResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        public async Task<IReadOnlyList<Image>> GetImagesAsync(params string[] tags)
        {
            string url;

            if (tags.Length == 0)
            {
                url = "/images";
            }
            else
            {
                var query = new StringBuilder();

                for (var i = 0; i < tags.Length; i++)
                {
                    var tag = tags[i];

                    if (string.IsNullOrEmpty(tag))
                    {
                        throw new ArgumentException("Cannot provide an empty tag name.", nameof(tags));
                    }

                    if (tag[0] == '-')
                    {
                        query.Append(tag);
                    }
                    else if (i > 0)
                    {
                        query.Append($"+{tag}");
                    }
                    else
                    {
                        query.Append(tag);
                    }
                }

                url = $"/images?tags={query}";
            }

            var response = await apiClient.GetAsync(url);
            ValidateResponse(response);

            var stream = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<Image[]>(stream);
        }

        public async Task<Image> GetImageAsync(ulong id)
        {
            var response = await apiClient.GetAsync($"/images/{id}");
            ValidateResponse(response);

            var stream = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<Image>(stream);
        }

        public async Task<Uri> PostImageAsync(Stream image, params string[] tags)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            // TODO: Check if we can stream this.

            var body = JsonSerializer.SerializeToUtf8Bytes(
                new PostImage
                {
                    Stream = image,
                    Tags = tags
                },
                JsonSerializerOptions
            );

            var content = new ByteArrayContent(body);
            content.Headers.ContentType = JsonHeaderValue;

            var response = await apiClient.PostAsync("/images", content);
            ValidateResponse(response);

            var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<ImagePostResult>(stream);

            return new Uri(result.File);
        }

        public class Config
        {
            public string Tenancy { get; set; } = "prod";

            public Uri Endpoint { get; set; } = new Uri("https://imgh.miki.ai/");

            public static Config Default()
            {
                return new Config();
            }
        }

        public void Dispose()
        {
            apiClient.Dispose();
        }
    }
}
