using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class NotificationRepositiory : INotificationRepositiory
	{
		public List<Notification> GetNotifications(int userId) => NotificationDAO.Instance.GetNotifications(userId);

        public Notification GetNotificationById(int notificationId) => NotificationDAO.Instance.GetNotificationById(notificationId);

        public void AddNotification(Notification notification) => NotificationDAO.Instance.AddNotification(notification);

        public void EditNotificationIsReaded(int notificationId) => NotificationDAO.Instance.EditNotificationIsReaded(notificationId);

    }
}
