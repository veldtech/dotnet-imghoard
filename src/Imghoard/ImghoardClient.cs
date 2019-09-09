using Imghoard.Models;
using Miki.Net.Http;
using Miki.Utils.Imaging.Headers;
using Miki.Utils.Imaging.Headers.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imghoard
{
    public class ImghoardClient
    {
        internal HttpClient APIClient;
        internal Uri API_Base;

        public ImghoardClient() : this(new Uri("https://imgh.miki.ai")) { }

        public ImghoardClient(string Endpoint) : this(new Uri(Endpoint)) { }

        public ImghoardClient(Uri Endpoint)
        {
            APIClient = new HttpClientFactory()
                .CreateNew();
#if DEBUG
            APIClient.AddHeader("x-miki-tenancy", "testing");
#endif
            API_Base = Endpoint;
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

            Uri url;

            if (query != null)
                url = new Uri(API_Base, $"/images?tags={query}");
            else
                url = new Uri(API_Base, $"/images");

            var response = await APIClient.GetAsync(url.ToString());

            if (response.Success)
            {
                return JsonConvert.DeserializeObject<IReadOnlyList<Image>>(response.Body);
            }

            throw new Exception(response.HttpResponseMessage.ReasonPhrase);
        }
        
        public async Task<Image> GetImageAsync(ulong Id)
        {
            var url = new Uri(API_Base, $"/images?id={Id}");

            var response = await APIClient.GetAsync(url.ToString());

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
                throw new NotSupportedException("You have given an incorrect image format, currently supported formats are: png, jpeg, gif");
            }

            var b64 = Convert.ToBase64String(bytes);
            image.Position = 0;

            var url = new Uri(API_Base, $"/images");

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

            var response = await APIClient.PostAsync(url.ToString(), body);

            if (response.Success)
            {
                return JsonConvert.DeserializeObject<Uri>(response.Body);
            }

            return await Task.FromResult<Uri>(null);
        }

        (bool, string) IsSupported(byte[] image)
        {
            if (ImageHeaders.Validate(image, ImageType.Png))
                return (true, "png");
            if (ImageHeaders.Validate(image, ImageType.Jpeg))
                return (true, "jpeg");
            if (ImageHeaders.Validate(image, ImageType.Gif89a) || ImageHeaders.Validate(image, ImageType.Gif87a))
                return (true, "gif");

            return (false, null);
        }
    }
}
