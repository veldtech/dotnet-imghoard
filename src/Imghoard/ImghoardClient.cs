using Imghoard.Exceptions;
using Imghoard.Models;
using Miki.Utils.Imaging.Headers;
using Miki.Utils.Imaging.Headers.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Imghoard
{
    public class ImghoardClient : IImghoardClient
    {
        private HttpClient apiClient;
        private readonly Config config;
        private const int Mb = 1000000;
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public ImghoardClient() : this(Config.Default()) { }

        public ImghoardClient(Config config)
        {
            this.config = config;

            apiClient = new HttpClient();
            apiClient.DefaultRequestHeaders.Add("x-miki-tenancy", config.Tenancy);
            apiClient.DefaultRequestHeaders.Add("User-Agent", config.UserAgent);
        }

        /// <summary>
        /// Gets the first page of results given an array of Tags to find
        /// </summary>
        /// <param name="Tags">Tags to search for</param>
        /// <returns>A readonly list of images found with the Tags entered</returns>
        public async Task<ImagesResponse> GetImagesAsync(params string[] Tags)
            => await GetImagesAsync(0, Tags);

        /// <summary>
        /// Gets the given page of results given an array of Tags to find
        /// </summary>
        /// <param name="Tags">Tags to search for</param>
        /// <returns>A readonly list of images found with the Tags entered</returns>
        public async Task<ImagesResponse> GetImagesAsync(int page = 0, params string[] Tags)
        {
            StringBuilder query = new StringBuilder();

            if (page > 0)
            {
                query.Append($"page={page}");
            }

            if (Tags.Any())
            {
                if (page > 0)
                {
                    query.Append("&tags=");
                }
                else
                {
                    query.Append("tags=");
                }

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

            StringBuilder url = new StringBuilder(config.Endpoint);

            if (query.Length > 0)
                url.Append($"images?{query}");
            else
                url.Append("images");

            var response = await apiClient.GetAsync(url.ToString());

            if (response.IsSuccessStatusCode)
            {
                return new ImagesResponse(this, JsonConvert.DeserializeObject<IReadOnlyList<Image>>(await response.Content.ReadAsStringAsync()), Tags, page);
            }

            throw new ResponseException("Response was not successfull; Reason: \"" + response.ReasonPhrase + "\"");
        }

        /// <summary>
        /// Get an image with a given Id
        /// </summary>
        /// <param name="Id">The snowflake Id of the Image to get</param>
        /// <returns>The image with the given snowflake</returns>
        public async Task<Image> GetImageAsync(ulong Id)
        {
            var url = new StringBuilder(config.Endpoint);
                
            url.Append($"images/{Id}");

            var response = await apiClient.GetAsync(url.ToString());

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Image>(await response.Content.ReadAsStringAsync());
            }

            throw new ResponseException(response.ReasonPhrase);
        }

        /// <summary>
        /// Posts a new image to the Imghoard instance
        /// </summary>
        /// <param name="image">The image stream to upload</param>
        /// <param name="Tags">The tags of the image being uploaded</param>
        /// <returns>The url of the uploaded image or null on failure</returns>
        public async Task<string> PostImageAsync(Stream image, params string[] Tags)
        {
            byte[] bytes;

            using (var mStream = new MemoryStream())
            {
                await image.CopyToAsync(mStream);
                bytes = mStream.ToArray();
            }
            image.Position = 0;

            return await PostImageAsync(bytes, Tags);
        }

        /// <summary>
        /// Posts a new image to the Imghoard instance
        /// </summary>
        /// <param name="bytes">The raw bytes of the image to upload</param>
        /// <param name="Tags">The tags of the image being uploaded</param>
        /// <returns>The url of the uploaded image or null on failure</returns>
        public async Task<string> PostImageAsync(Memory<byte> bytes, params string[] Tags)
        {
            if(bytes.Length >= Mb && !config.Experimental)
            {
                throw new NotSupportedException("In order to upload images larger than 1MB you need to enable experimental features in the config");
            }

            (bool supported, string prefix) = IsSupported(bytes.Span);

            if (!supported)
            {
                throw new NotSupportedException("You have given an incorrect image format, currently supported formats are: png, jpeg, gif");
            }

            var url = config.Endpoint + "images";

            if(bytes.Length < Mb)
            {
                var body = JsonConvert.SerializeObject(
                        new PostImage
                        {
                            Data = $"data:image/{prefix};base64,{Convert.ToBase64String(bytes.Span)}",
                            Tags = Tags
                        },
                        serializerSettings
                );

                var response = await apiClient.PostAsync(url, new StringContent(body));

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<UploadResponse>(await response.Content.ReadAsStringAsync()).File;
                }

                throw new ResponseException("Response was not successfull; Reason: \"" + response.ReasonPhrase + "\"");
            }
            else
            {
                var body = new MultipartFormDataContent
                {
                    { new StringContent($"image/{prefix}"), "data-type" },
                    { new ByteArrayContent(bytes.Span.ToArray()), "data" },
                    { new StringContent(string.Join(",", Tags)), "tags" }
                };

                var response = await apiClient.PostAsync(url, body);

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<UploadResponse>(await response.Content.ReadAsStringAsync()).File;
                }

                throw new ResponseException("Response was not successfull; Reason: \"" + response.ReasonPhrase + "\"");
            }
        }

        (bool, string) IsSupported(Span<byte> image)
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
    }
}
