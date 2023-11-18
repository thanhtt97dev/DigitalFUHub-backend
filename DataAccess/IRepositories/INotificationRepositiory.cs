using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface INotificationRepositiory
	{
		List<Notification> GetNotifications(long userId, int index);

        Notification GetNotificationById(int notificationId);

        void AddNotification(Notification notification);

        void EditNotificationIsReaded(int notificationId);

        void EditReadAllNotifications(int userId);
		int GetTotalNumberNotification(long userId);
		int GetTotalNumberNotificationUnRead(long userId);
	}
}
