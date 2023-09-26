using Azure.Core;
using Azure.Storage.Blobs;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;

namespace DigitalFUHubApi.Services
{
	public class StorageService
	{
		private readonly IConfiguration _configuration;

		private readonly string connectionString = "";
		private readonly string containerName = "";
		public StorageService(IConfiguration configuration)
		{
			_configuration = configuration;
			connectionString = _configuration["Azure:StorageConnectionString"] ?? "";
			containerName = _configuration["Azure:StorageContainerName"] ?? "";
		}

		public async Task<string> UploadFileToAzureAsync(IFormFile fileUpload, string filename)
		{
			try
			{
				BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
				await container.UploadBlobAsync(filename, fileUpload.OpenReadStream());
				return string.Format("{0}/{1}/{2}", Constants.AZURE_ROOT_PATH,containerName,filename);
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
		public async Task<Stream?> GetFileFromAzureAsync(string filename)
		{
			BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
			BlobClient blobClient = container.GetBlobClient(filename);
			bool isExist = await blobClient.ExistsAsync();
			if (!isExist)
			{
				return null;
			}
			Stream streamContent = await blobClient.OpenReadAsync();
			return streamContent;
		}
		public async Task<bool> RemoveFileFromAzureAsync(string filename)
		{
			BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
			BlobClient blobClient = container.GetBlobClient(filename);
			bool isExist = await blobClient.ExistsAsync();
			if (!isExist)
			{
				return false;
			}
			await blobClient.DeleteAsync();
			return true;
		}
	}
}
