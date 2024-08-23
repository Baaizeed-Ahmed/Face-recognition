using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Linq;

namespace EmployeeRecognitionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageRecognitionController : ControllerBase
    {
        private readonly string _imagesDir = Path.Combine(Directory.GetCurrentDirectory(), "Images");

        [HttpPost("upload")]
        public IActionResult UploadImage([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image provided.");

            var uploadedImageData = ConvertImageToByteArray(image);

            if (uploadedImageData == null)
                return BadRequest("Invalid image format.");

            foreach (var filePath in Directory.GetFiles(_imagesDir))
            {
                var storedImageData = ConvertImageToByteArray(filePath);

                if (uploadedImageData.SequenceEqual(storedImageData))
                {
                    return Ok(new { message = "Employee recognized" });
                }
            }

            return Ok(new { message = "Unrecognized person" });
        }

        private byte[] ConvertImageToByteArray(IFormFile image)
        {
            using var stream = new MemoryStream();
            image.CopyTo(stream);
            return ResizeImage(stream.ToArray());
        }

        private byte[] ConvertImageToByteArray(string filePath)
        {
            var image = System.IO.File.ReadAllBytes(filePath);
            return ResizeImage(image);
        }

        private byte[] ResizeImage(byte[] imageBytes)
        {
            using var image = Image.Load<Rgba32>(imageBytes);
            image.Mutate(x => x.Resize(200, 200));
            using var ms = new MemoryStream();
            image.SaveAsJpeg(ms);
            return ms.ToArray();
        }
    }
}
