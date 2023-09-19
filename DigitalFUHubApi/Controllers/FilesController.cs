using Azure.Storage.Blobs;
using BusinessObject;
using DataAccess.IRepositories;
using DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FilesController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		private readonly IStorageRepository _storageRepository;
		private readonly string connectionString = "";
		private readonly string containerName = "";
		public FilesController(IConfiguration configuration, IStorageRepository storageRepository)
		{
			_configuration = configuration;
			_storageRepository = storageRepository;
			connectionString = _configuration["Azure:StorageConnectionString"] ?? "";
			containerName = _configuration["Azure:StorageContainerName"] ?? "";
		}

		[HttpPost]
		[Route("Upload")]
		public async Task<IActionResult> Upload([FromForm] FileUploadRequestDTO request)
		{
			try
			{
				BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
				string fNameExtension = request.FileUpload.FileName.Substring(request.FileUpload.FileName.LastIndexOf("."));
				string filename = string.Concat(Guid.NewGuid().ToString(), fNameExtension);
				await container.UploadBlobAsync(filename, request.FileUpload.OpenReadStream());

				_storageRepository.AddFile(new Storage
				{
					FileName = filename,
					UserId = request.UserId,
					IsPublic = request.IsPublic
				});
			}
			catch (Exception)
			{

				return Conflict();
			}
			return Ok();
		}
		[HttpGet]
		[Route("GetFile/{filename}")]
		public async Task<IActionResult> GetFile(string filename)
		{
			//Storage? file = _storageRepository.GetFileByName(filename);
			//if (file == null)
			//{
			//	return NotFound();
			//}
			//else if (!file.IsPublic)
			//{
			//	return BadRequest();
			//}
			BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
			Stream? streamContent = await _storageRepository.GetFileFromAzureAsync(container.GetBlobClient(filename).Uri.ToString());
			if (streamContent == null)
			{
				return NotFound();
			}
			var contentType = "";
			string fileNameExtension = filename.Substring(filename.LastIndexOf("."));
			if (fileNameExtension.Contains(".jpg") || fileNameExtension.Contains(".jpeg"))
			{
				contentType = "image/jpeg";
			}
			else if (fileNameExtension.Contains(".png"))
			{
				contentType = "image/png";
			}
			else if (fileNameExtension.Contains(".gif"))
			{
				contentType = "image/gif";
			}
			else if (fileNameExtension.Contains(".txt"))
			{
				contentType = "text/xml";
			}
			else if (fileNameExtension.Contains(".mp3") || fileNameExtension.Contains(".mp4"))
			{
				contentType = "audio/mpeg";
			}
			else
			{
				contentType = "application/octet-stream";
			}
			return File(streamContent, contentType); ;
		}
	}
}
