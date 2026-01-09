using FarmAPI.Models;
using FarmAPI.Models.Dtos;
using FarmAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FarmAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly FileStoreSettings fileStoreSettings;

        public FileUploadController(IOptions<FileStoreSettings> settings)
        {
            fileStoreSettings = settings.Value;
        }

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

            //var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            var uploadsFolder = fileStoreSettings.BaseFolderPath;
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = string.Empty;
            string fullFilePath = string.Empty;

            if (file.ContentType.Contains("image"))
            {
                filePath = Path.Combine(uploadsFolder, fileStoreSettings.ImagesFolderName);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
            } 
            else if (file.ContentType.Contains("audio"))
            {
                filePath = Path.Combine(uploadsFolder, fileStoreSettings.AudioFolderName);
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