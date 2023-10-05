using BusinessObject;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class CategoryDAO
	{
		private static CategoryDAO? instance;
		private static readonly object instanceLock = new object();
		public static CategoryDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new CategoryDAO();
					}
				}
				return instance;
			}
		}

		internal List<Category> GetAll()
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Category.Select(x => new Category
				{
					CategoryId = x.CategoryId,
					CategoryName = x.CategoryName,
				}).ToList();
			}
		}
	}
}
