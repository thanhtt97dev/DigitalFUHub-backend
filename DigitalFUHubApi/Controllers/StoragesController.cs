using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using BusinessObject.Entities;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StoragesController : ControllerBase
	{
		private readonly StorageService _storageService;
		private readonly IStorageRepository _storageRepository;
		public StoragesController(StorageService storageService, IStorageRepository storageRepository)
		{
			_storageRepository = storageRepository;
			_storageService = storageService;
		}

		[HttpPost]
		[Route("Upload")]
		public async Task<IActionResult> Upload([FromForm] FileUploadRequestDTO request)
		{
			try
			{
				string fileExtension = request.FileUpload.FileName.Substring(request.FileUpload.FileName.LastIndexOf("."));
				string filename = string.Concat(Guid.NewGuid().ToString(), fileExtension);
				await _storageService.UploadFileToAzureAsync(request.FileUpload, filename);

				_storageRepository.AddFile(new Media
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
			
			Stream? streamContent = await _storageService.GetFileFromAzureAsync(filename);
			if (streamContent == null)
			{
				return NotFound();
			}
			var contentType = Util.GetContentType(filename);
			return File(streamContent, contentType);
		}
		[HttpDelete]
		[Route("RemoveFile/{filename}")]
		public async Task<IActionResult> RemoveFile(string filename)
		{
			try
			{
				bool isRemove = await _storageService.RemoveFileFromAzureAsync(filename);
				if(!isRemove)
				{
					return NotFound();
				}
				_storageRepository.RemoveFile(filename);
			}
			catch (Exception)
			{
				return Conflict();
			}
			return Ok(); 
		}
	}
}
