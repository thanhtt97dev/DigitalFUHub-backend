using BusinessObject.Entities;
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


		internal void RemoveFile(string filename)
		{
			Storage? file = GetFileByName(filename);
			if (file != null)
			{
				using (ApiContext context = new ApiContext())
				{
					context.Storage.Remove(file);
					context.SaveChanges();
				}
			}
		}
	}
}
