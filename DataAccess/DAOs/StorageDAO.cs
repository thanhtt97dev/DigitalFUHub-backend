using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class StorageDAO
	{
		private static StorageDAO? instance;
		private static readonly object instanceLock = new object();
		public static StorageDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new StorageDAO();
					}
				}
				return instance;
			}
		}

		public void AddFile(Storage file)
		{
			using (ApiContext context = new ApiContext())
			{
				try
				{
					context.Storage.Add(file);
					context.SaveChanges();
				}
				catch (Exception e)
				{

					throw new Exception(e.Message);
				}
			}
		}

		internal Storage? GetFileByName(string filename)
		{
			using (ApiContext context = new ApiContext())
			{
				return context.Storage.FirstOrDefault(x => x.FileName == filename);
			}
		}

		internal async Task<Stream?> GetFileFromAzureAsync(string uri)
		{
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			HttpResponseMessage response = await client.GetAsync(uri);
			if (!response.IsSuccessStatusCode) return null;
			Stream content = await response.Content.ReadAsStreamAsync();
			return content;
		}
	}
}
