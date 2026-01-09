using FarmAPI.Models;
using FarmAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.IO;

namespace EasyGrow.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/admin/[controller]")]
    //[ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger unless admin
    public class LgdController : ControllerBase
    {
        private readonly LgdImportService _importService;
        private readonly FileStoreSettings fileStoreSettings;

        public LgdController(IOptions<FileStoreSettings> settings, LgdImportService importService)
        {
            _importService = importService;
            fileStoreSettings = settings.Value;
        }

        //[HttpPost("import")]
        //public async Task<IActionResult> ImportCsv(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest("No file uploaded");

        //    var tempPath = Path.GetTempFileName();
        //    try
        //    {
        //        using (var stream = new FileStream(tempPath, FileMode.Create))
        //        {
        //            await file.CopyToAsync(stream);
        //        }

        //        var sourceFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.UtcNow:yyyy-MM}.csv";
        //        var count = await _importService.ImportStateCsvAsync(tempPath, sourceFileName);

        //        return Ok(new { imported = count, sourceFile = sourceFileName });
        //    }
        //    finally
        //    {
        //        if (System.IO.File.Exists(tempPath))
        //            System.IO.File.Delete(tempPath);
        //    }
        //}

        //[HttpPost("synccsv")]
        //[DisableRequestSizeLimit]  // For large LGD files
        //public async Task<IActionResult> SyncCsv(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest("No CSV file uploaded");

        //    if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        //        return BadRequest("Only CSV files allowed");

        //    var sourceFileName = file.FileName;
        //    using var stream = file.OpenReadStream();

        //    try
        //    {
        //        var count = await _importService.SyncCsvFromStreamAsync(stream, sourceFileName);
        //        return Ok(new
        //        {
        //            Success = true,
        //            Synced = count,
        //            File = sourceFileName
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("CSV sync failed for {File}", sourceFileName);
        //        return StatusCode(500, $"Sync failed: {ex.Message}");
        //    }
        //}


        [HttpPost("FullLoadCsv")]
        public async Task<IActionResult> FullLoadCsv()
        {
            string downloadsFolderPath, processedFolderPath, fullFilePath, message = string.Empty;

            var filesFolder = fileStoreSettings.BaseFolderPath;
            var downloadedFolder = fileStoreSettings.LGDDownloadedFolderName;
            var processedFolder = fileStoreSettings.LGDProcessedFolderName;

            if (!Directory.Exists(filesFolder))
            {
                Directory.CreateDirectory(filesFolder);
            }

            downloadsFolderPath = Path.Combine(filesFolder, downloadedFolder);
            processedFolderPath = Path.Combine(filesFolder, processedFolder);

            if (!Directory.Exists(downloadsFolderPath))
            {
                Directory.CreateDirectory(downloadsFolderPath);
            }

            if (!Directory.Exists(processedFolderPath))
            {
                Directory.CreateDirectory(processedFolderPath);
            }

            //check if any files present in the folder and then iterate over all and import
            if (!Directory.GetFiles(downloadsFolderPath).Any())
            {
                Console.WriteLine("No files found in the LGD Import folder.");
                return BadRequest("No files found in the LGD Import folder.");
            }

            var downloadedFiles = Directory.EnumerateFiles(downloadsFolderPath);
            long totalCount = 0;

            foreach (var file in downloadedFiles)
            {
                try
                {
                    var processedFileName = $"{Path.GetFileNameWithoutExtension(file)}_{DateTime.UtcNow:dd-MM-yyyy-HH-mm-ss}.csv";
                    var processedFile = Path.Combine(processedFolderPath, processedFileName);
                    System.IO.File.Copy(file, processedFile);

                    var countByFile = await _importService.ImportStateCsvAsync(file, processedFileName);
                    totalCount += countByFile;
                    message += $"Imported {countByFile} records from {processedFileName}\n";
                    Console.WriteLine($"Imported {countByFile} records from {processedFileName}");
                }
                catch (Exception ex)
                {
                    message += $"Error importing file {file}: {ex.Message}\n";
                    Console.WriteLine($"Error importing file {file}: {ex.Message}");
                    continue;
                }
            }
            message += $"Total Imported Records: {totalCount}";

            return Ok(message);
        }
    }
}
