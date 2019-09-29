using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Imghoard.Models;
using Miki.Net.Http;
using Miki.Utils.Imaging.Headers;
using Miki.Utils.Imaging.Headers.Models;
using Newtonsoft.Json;

namespace Imghoard
{
    public class ImghoardClient
    {
        private HttpClient apiClient;
        private Config config;

        public ImghoardClient(ImghoardClient.Config config) {
            this.config = config;
        }

        public ImghoardClient(Uri Endpoint)
        {
            apiClient = new HttpClientFactory()
                .HasBaseUri(config.Endpoint)
                .CreateNew();
            apiClient.AddHeader("x-miki-tenancy", config.Tenancy);
        }

        public async Task<IReadOnlyList<Image>> GetImagesAsync(params string[] Tags)
        {
            StringBuilder query = null;
            if (Tags.Any())
            {
                query = new StringBuilder();
                foreach (string tag in Tags)
                {
                    if (tag.StartsWith("-"))
                    {
                        query.Append(tag);
                        continue;
                    }
                    else
                    {
                        if (tag != Tags.FirstOrDefault())
                        {
                            query.Append($"+{tag}");
                            continue;
                        }

                        query.Append(tag);
                    }
                }
            }


            StringBuilder urlBuilder = new StringBuilder("/images");
            if(query != null)
            {
                urlBuilder.Append($"?tags={query}");
            }
            var response = await apiClient.GetAsync(urlBuilder.ToString());

            if (response.Success)
            {
                return JsonConvert.DeserializeObject<IReadOnlyList<Image>>(response.Body);
            }

            // TODO(velddev): Add better error handling.
            throw new Exception(response.HttpResponseMessage.ReasonPhrase);
        }
        
        public async Task<Image> GetImageAsync(ulong Id)
        {
            var response = await apiClient.GetAsync("/images?id={Id}");
            if (response.Success)
            {
                return JsonConvert.DeserializeObject<Image>(response.Body);
            }
            throw new Exception(response.HttpResponseMessage.ReasonPhrase);
        }

        public async Task<Uri> PostImageAsync(Stream image, params string[] Tags)
        {
            byte[] bytes;

            using (var mStream = new MemoryStream())
            {
                await image.CopyToAsync(mStream);
                bytes = mStream.ToArray();
            }

            var imgd = IsSupported(bytes);

            if (!imgd.Item1)
            {
                throw new NotSupportedException(
                    "You have given an incorrect image format, currently supported formats are: png, jpeg, gif");
            }

            var b64 = Convert.ToBase64String(bytes);
            image.Position = 0;

            var body = JsonConvert.SerializeObject(
                new PostImage
                {
                    Data = $"data:image/{imgd.Item2};base64,{b64}",
                    Tags = Tags
                },
                new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                }
            );

            var response = await apiClient.PostAsync("/images", body);

            if (response.Success)
            {
                return JsonConvert.DeserializeObject<Uri>(response.Body);
            }
            return null;
        }

        (bool, string) IsSupported(byte[] image)
        {
            if(ImageHeaders.Validate(image, ImageType.Png))
            {
                return (true, "png");
            }
            if(ImageHeaders.Validate(image, ImageType.Jpeg))
            {
                return (true, "jpeg");
            }
            if(ImageHeaders.Validate(image, ImageType.Gif89a) 
                || ImageHeaders.Validate(image, ImageType.Gif87a))
            {
                return (true, "gif");
            }
            return (false, null);
        }

        public class Config
        {
            public string Tenancy { get; set; } = "prod";
            public string Endpoint { get; set; } = "https://imgh.miki.ai/";

            public static Config Default()
            {
                return new Config();
            }
        }
    }
}
