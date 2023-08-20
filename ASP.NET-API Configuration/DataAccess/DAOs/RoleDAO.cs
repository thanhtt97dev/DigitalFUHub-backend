using BusinessObject;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class RoleDAO
	{
		private static RoleDAO? instance;
		private static readonly object instanceLock = new object();

		public static RoleDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new RoleDAO();
					}
				}
				return instance;
			}
		}

		internal List<Role> GetAllRole()
		{
			using (ApiContext context = new ApiContext())
			{
				return context.Role.ToList();
			}
		}
	}
}
