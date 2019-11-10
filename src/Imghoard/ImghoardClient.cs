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
    public class ImghoardClient : IImghoardClient
    {
        private HttpClient apiClient;
        private readonly Config config;

        public ImghoardClient() : this(Config.Default()) { }

        public ImghoardClient(Config config)
        {
            this.config = config;
            
            apiClient = new HttpClientFactory()
                .HasBaseUri(config.Endpoint)
                .CreateNew();

            apiClient.AddHeader("x-miki-tenancy", config.Tenancy);
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
                url.Append($"/images?{query}");
            else
                url.Append("/images");

            var response = await apiClient.GetAsync(url.ToString());

            if (response.Success)
            {
                return new ImagesResponse(this, JsonConvert.DeserializeObject<IReadOnlyList<Image>>(response.Body), Tags, page);
            }

            throw new Exception(response.HttpResponseMessage.ReasonPhrase);
        }

        /// <summary>
        /// Get an image with a given Id
        /// </summary>
        /// <param name="Id">The snowflake Id of the Image to get</param>
        /// <returns>The image with the given snowflake</returns>
        public async Task<Image> GetImageAsync(ulong Id)
        {
            var url = new StringBuilder(config.Endpoint);
                
            url.Append($"/images/{Id}");

            var response = await apiClient.GetAsync(url.ToString());

            if (response.Success)
            {
                return JsonConvert.DeserializeObject<Image>(response.Body);
            }

            throw new Exception(response.HttpResponseMessage.ReasonPhrase);
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
            (bool supported, string prefix) = IsSupported(bytes.Span.ToArray());

            if (!supported)
            {
                throw new NotSupportedException("You have given an incorrect image format, currently supported formats are: png, jpeg, gif");
            }

            var url = config.Endpoint + "/images";

            var body = JsonConvert.SerializeObject(
                new PostImage
                {
                    Data = $"data:image/{prefix};base64,{Convert.ToBase64String(bytes.Span)}",
                    Tags = Tags
                },
                new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                }
            );

            var response = await apiClient.PostAsync(url, body);

            if (response.Success)
            {
                return JsonConvert.DeserializeObject<UploadResponse>(response.Body).File;
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
    }
}
