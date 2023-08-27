using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface IStorageRepository
	{
		void AddFile(Storage file);
		Storage? GetFileByName(string filename);
		Task<Stream?> GetFileFromAzureAsync(string fileName);
	}
}
