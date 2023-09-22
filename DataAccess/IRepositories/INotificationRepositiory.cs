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
		List<Notification> GetNotifications(int userId);

		void AddNotification(Notification notification);
	}
}
