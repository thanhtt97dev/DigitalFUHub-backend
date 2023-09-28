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
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Role.ToList();
			}
		}

		internal Role GetAllRole(long id)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Role.First(x => x.RoleId == id);
			}
		}
	}
}
