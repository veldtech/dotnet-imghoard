using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Miki.Utils.Imaging.Headers;
using Miki.Utils.Imaging.Headers.Models;

namespace Imghoard.Formatters
{
    internal sealed class Base64FileFormatter : JsonConverter<Stream>
    {
        public override Stream Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException("Reading a Base64 stream is not supported.");
        }

        public override void Write(Utf8JsonWriter writer, Stream value, JsonSerializerOptions options)
        {
            byte[] data;

            if (value is MemoryStream memoryStream)
            {
                data = memoryStream.ToArray();
            }
            else
            {
                // TODO: Check if this can be improved.
                using var ms = new MemoryStream();
                value.Position = 0;
                value.CopyTo(ms);
                data = ms.ToArray();
            }

            var (supported, name) = IsSupported(data);

            if (!supported)
            {
                throw new NotSupportedException($"File extension {name} is not supported. Supported extensions: png, jpeg, gif.");
            }

            var prefix = $"data:image/{name};base64,";

            var prefixLength = Encoding.UTF8.GetByteCount(prefix);
            var encodingLength = Base64.GetMaxEncodedToUtf8Length(data.Length);
            var length = prefixLength + encodingLength;

            var destination = ArrayPool<byte>.Shared.Rent(length);
            var span = destination.AsSpan();

            try
            {
                Encoding.UTF8.GetBytes(prefix, span);

                var status = Base64.EncodeToUtf8(data, span.Slice(prefixLength), out var consumed, out var written);
                Debug.Assert(status == OperationStatus.Done);
                Debug.Assert(consumed == data.Length);

                writer.WriteStringValue(span.Slice(0, prefixLength + written));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(destination);
            }
        }

        private static (bool supported, string name) IsSupported(byte[] image)
        {
            if (ImageHeaders.Validate(image, ImageType.Png))
            {
                return (true, "png");
            }

            if (ImageHeaders.Validate(image, ImageType.Jpeg))
            {
                return (true, "jpeg");
            }

            if (ImageHeaders.Validate(image, ImageType.Gif89a) || ImageHeaders.Validate(image, ImageType.Gif87a))
            {
                return (true, "gif");
            }

            return (false, null);
        }
    }
}