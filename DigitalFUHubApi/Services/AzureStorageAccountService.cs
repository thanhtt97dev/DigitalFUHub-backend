using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using System.Net.Http.Headers;
using Comons;

namespace DigitalFUHubApi.Services
{
	public class AzureStorageAccountService
	{
		private IConfiguration configuration;

		public AzureStorageAccountService(IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public async Task<string> UploadFileToAzureAsync(IFormFile fileUpload, string filename)
		{
			try
			{
				var azureStorageEndPoint = configuration[Constants.AZURE_STORAGE_END_POINT];
				var azureStorageConnectionString = configuration[Constants.AZURE_STORAGE_CONNECTION_STRING];
				var azureStorageContainerName = configuration[Constants.AZURE_STORAGE_CONTAINER_NAME];

				BlobContainerClient container = new BlobContainerClient(azureStorageConnectionString, azureStorageContainerName);
				var blobClient = container.GetBlobClient(filename);
				var blobHttpHeader = new BlobHttpHeaders { ContentType = Util.Instance.GetContentType(filename) };
				await blobClient.UploadAsync(fileUpload.OpenReadStream(), new BlobUploadOptions { HttpHeaders = blobHttpHeader });
				return string.Format("{0}/{1}/{2}", azureStorageEndPoint, azureStorageContainerName, filename);
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
		public async Task<Stream?> GetFileFromAzureAsync(string filename)
		{
			BlobContainerClient container = new BlobContainerClient(Constants.AZURE_STORAGE_CONNECTION_STRING, Constants.AZURE_STORAGE_CONTAINER_NAME);
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
			BlobContainerClient container = new BlobContainerClient(Constants.AZURE_STORAGE_CONNECTION_STRING, Constants.AZURE_STORAGE_CONTAINER_NAME);
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
