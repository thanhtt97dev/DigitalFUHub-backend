using BusinessObject;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class NotificationDAO
	{
		private static NotificationDAO? instance;
		private static readonly object instanceLock = new object();

		public static NotificationDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new NotificationDAO();
					}
				}
				return instance;
			}
		}

		internal List<Notification> GetNotifications(int userId)
		{
			List<Notification> notifications = new List<Notification>();	
			using (ApiContext context = new ApiContext())
			{
				notifications = context.Notification.Where(x => x.UserId == userId).ToList();
			}
			return notifications;	
		}

		internal void AddNotification(Notification notification)
		{
			using (ApiContext context = new ApiContext())
			{
				context.Notification.Add(notification);
				context.SaveChanges();
			}
		}
	}
}
