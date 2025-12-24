using FarmAPI.Models;
using FarmAPI.Models.Dtos;
using FarmAPI.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        //public FileUploadController(IWebHostEnvironment webHostEnvironment)
        //{
        //    _webHostEnvironment = webHostEnvironment;
        //}

        /// <summary>
        /// Uploads a file to the server and returns the file name and path.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>fileName, fullPath</returns>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = string.Empty;
            string fullFilePath = string.Empty;

            if (file.ContentType.Contains("image"))
            {
                filePath = Path.Combine(uploadsFolder, "images");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
            } 
            else if (file.ContentType.Contains("audio"))
            {
                filePath = Path.Combine(uploadsFolder, "audio");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
            }

            fullFilePath = Path.Combine(filePath, uniqueFileName);
            // Save the file to the server
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }


            return Ok(new { fileName = uniqueFileName, fullPath = fullFilePath });
        }
    }
}