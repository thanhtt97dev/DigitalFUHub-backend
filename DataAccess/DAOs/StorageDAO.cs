using BusinessObject;
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
		//private static StorageDAO? instance;
		//private static readonly object instanceLock = new object();
		//public static StorageDAO Instance
		//{
		//	get
		//	{
		//		lock (instanceLock)
		//		{
		//			if (instance == null)
		//			{
		//				instance = new StorageDAO();
		//			}
		//		}
		//		return instance;
		//	}
		//}

		//public void AddFile(Media file)
		//{
		//	using (DatabaseContext context = new DatabaseContext())
		//	{
		//		try
		//		{
		//			context.Media.Add(file);
		//			context.SaveChanges();
		//		}
		//		catch (Exception e)
		//		{

		//			throw new Exception(e.Message);
		//		}
		//	}
		//}

		//internal Media? GetFileByName(string filename)
		//{
		//	using (DatabaseContext context = new DatabaseContext())
		//	{
		//		return context.Media.FirstOrDefault(x => x.Url.Substring(x.Url.LastIndexOf("/") + 1) == filename);
		//	}
		//}


		//internal void RemoveFile(string filename)
		//{
		//	Media? file = GetFileByName(filename);
		//	if (file != null)
		//	{
		//		using (DatabaseContext context = new DatabaseContext())
		//		{
		//			context.Media.Remove(file);
		//			context.SaveChanges();
		//		}
		//	}
		//}
	}
}
