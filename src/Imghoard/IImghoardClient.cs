using Imghoard.Models;
using System;
using System.Threading.Tasks;

namespace Imghoard
{
    public interface IImghoardClient
    {
        Task<ImagesResponse> GetImagesAsync(params string[] Tags);
        Task<ImagesResponse> GetImagesAsync(int page = 0, params string[] Tags);
        Task<Image> GetImageAsync(ulong Id);
        Task<string> PostImageAsync(Memory<byte> bytes, params string[] Tags);
    }
}
