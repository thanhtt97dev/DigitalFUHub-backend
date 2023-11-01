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

		internal List<Notification> GetNotifications(long userId, int offset)
		{
			List<Notification> notifications = new List<Notification>();	
			using (DatabaseContext context = new DatabaseContext())
			{
				notifications = context.Notification.Where(x => x.UserId == userId)
					.OrderByDescending(x => x.DateCreated).Skip(offset).Take(5).ToList();
			}
			return notifications;	
		}

        internal Notification GetNotificationById(int notificationId)
        {
            Notification notification = new Notification();
            using (DatabaseContext context = new DatabaseContext())
            {
				notification = context.Notification.First(x => x.NotificationId == notificationId);
            }
            return notification;
        }

        internal void AddNotification(Notification notification)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				context.Notification.Add(notification);
				context.SaveChanges();
			}
		}

        internal void EditNotificationIsReaded(int notificationId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var notification = context.Notification.First(x => x.NotificationId == notificationId);
				notification.IsReaded = true;
                context.SaveChanges();
            }
        }

        internal void EditReadAllNotifications(int userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var notifications = context.Notification.Where(x => x.UserId == userId).ToList();
                foreach (var notification in notifications)
				{
					notification.IsReaded = true;
				}
                context.SaveChanges();
            }
        }
    }
}
