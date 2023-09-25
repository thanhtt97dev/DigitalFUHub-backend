using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface IStorageRepository
	{
		void AddFile(Media file);
		Media? GetFileByName(string filename);
		void RemoveFile(string filename);
	}
}
