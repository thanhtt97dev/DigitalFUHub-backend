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
		public List<Notification> GetNotifications(long userId, int index) => NotificationDAO.Instance.GetNotifications(userId, index);

        public Notification GetNotificationById(int notificationId) => NotificationDAO.Instance.GetNotificationById(notificationId);

        public void AddNotification(Notification notification) => NotificationDAO.Instance.AddNotification(notification);

        public void EditNotificationIsReaded(int notificationId) => NotificationDAO.Instance.EditNotificationIsReaded(notificationId);

        public void EditReadAllNotifications(int userId) => NotificationDAO.Instance.EditReadAllNotifications(userId);

		public int GetTotalNumberNotification(long userId) => NotificationDAO.Instance.GetTotalNumberNotification(userId);

		public int GetTotalNumberNotificationUnRead(long userId) => NotificationDAO.Instance.GetTotalNumberNotificationUnRead(userId);

	}
}
