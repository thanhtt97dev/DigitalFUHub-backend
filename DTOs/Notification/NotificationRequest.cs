using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Notification
{
    public class NotificationRequest
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
