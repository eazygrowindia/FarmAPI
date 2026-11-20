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
        /// Supports images, audio, and documents (PDF, etc. for water test certificates).
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

            var uploadsFolder = fileStoreSettings.BaseFolderPath;
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = string.Empty;
            string fullFilePath = string.Empty;

            var extension = Path.GetExtension(file.FileName).ToLower();
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var audioExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac", ".flac", ".ogg", ".webm" };

            if (file.ContentType.Contains("image") || imageExtensions.Contains(extension))
            {
                filePath = Path.Combine(uploadsFolder, fileStoreSettings.ImagesFolderName ?? "Images");
            } 
            else if (file.ContentType.Contains("audio") || audioExtensions.Contains(extension))
            {
                filePath = Path.Combine(uploadsFolder, fileStoreSettings.AudioFolderName ?? "Audio");
            }
            else
            {
                // Documents & certificates (e.g. PDF, doc, docx for water test certificates)
                filePath = Path.Combine(uploadsFolder, "Documents");
            }

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            fullFilePath = Path.Combine(filePath, uniqueFileName);
            // Save the file to the server
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { fileName = uniqueFileName, fullPath = fullFilePath });
        }

        /// <summary>
        /// Downloads a file from the server by file name.
        /// Supports image, audio, voice, and document files (including water test certificates).
        /// </summary>
        /// <param name="fileName">The file name with extension (e.g., ffdac3fc-b177-46a9-8a51-5e8a60c8b0bd.jpg or certificate.pdf)</param>
        /// <returns>The file content with appropriate content type</returns>
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            var uploadsFolder = fileStoreSettings.BaseFolderPath;
            var cleanFileName = Path.GetFileName(fileName);
            var extension = Path.GetExtension(cleanFileName).ToLower();

            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var audioExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac", ".flac", ".ogg", ".webm" };
            var documentExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".csv", ".xlsx" };

            string filePath = string.Empty;

            if (imageExtensions.Contains(extension))
            {
                filePath = Path.Combine(uploadsFolder, fileStoreSettings.ImagesFolderName ?? "Images", cleanFileName);
            }
            else if (audioExtensions.Contains(extension))
            {
                filePath = Path.Combine(uploadsFolder, fileStoreSettings.AudioFolderName ?? "Audio", cleanFileName);
            }
            else if (documentExtensions.Contains(extension))
            {
                filePath = Path.Combine(uploadsFolder, "Documents", cleanFileName);
                if (!System.IO.File.Exists(filePath))
                {
                    // Fallback locations in case the document was saved directly under BaseFolderPath or current directory
                    var baseFallback = Path.Combine(uploadsFolder, cleanFileName);
                    if (System.IO.File.Exists(baseFallback))
                    {
                        filePath = baseFallback;
                    }
                    else
                    {
                        var rootFallback = Path.Combine(Directory.GetCurrentDirectory(), cleanFileName);
                        if (System.IO.File.Exists(rootFallback))
                        {
                            filePath = rootFallback;
                        }
                    }
                }
            }
            else
            {
                return BadRequest("Unsupported file type.");
            }

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            // Determine content type
            var contentType = GetContentType(extension);

            // Read the file and return it
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, contentType, cleanFileName);
        }

        /// <summary>
        /// Determines the content type based on file extension.
        /// </summary>
        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                // Image types
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                // Audio types
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".m4a" => "audio/mp4",
                ".aac" => "audio/aac",
                ".flac" => "audio/flac",
                ".ogg" => "audio/ogg",
                ".webm" => "audio/webm",
                // Document types
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                _ => "application/octet-stream"
            };
        }
    }
}