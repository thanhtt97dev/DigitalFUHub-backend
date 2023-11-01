using AutoMapper;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using DigitalFUHubApi.Hubs;
using DigitalFUHubApi.Managers;
using DigitalFUHubApi.Comons;
using BusinessObject.Entities;
using DTOs.Notification;
using Comons;
using DataAccess.Repositories;
using DTOs.User;
using System.Collections.Generic;
using DTOs.Order;
using DigitalFUHubApi.Services;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class NotificationsController : ControllerBase
	{
		private readonly IHubContext<NotificationHub> _hubContext;

		private readonly IConnectionManager _connectionManager;

		private readonly INotificationRepositiory _notificationRepositiory;

		private readonly IMapper _mapper;

		public NotificationsController(IHubContext<NotificationHub> hubContext, IConnectionManager connectionManager,
			INotificationRepositiory notificationRepositiory, IMapper mapper)
		{
			_hubContext = hubContext;
			_connectionManager = connectionManager;
			_notificationRepositiory = notificationRepositiory;
			_mapper = mapper;
		}

		#region Send notification to a user
		//[Authorize]
		[HttpPost("sendNotification/{userId}")]
		public async Task<IActionResult> SendNotification([FromRoute] int userId, NotificationRequest notificationRequest)
		{
			try
			{
				HashSet<string>? connections = _connectionManager
					.GetConnections(userId, Constants.SIGNAL_R_NOTIFICATION_HUB);

				Notification notification = new Notification()
				{
					UserId = userId,
					Title = notificationRequest.Title,
					Content = notificationRequest.Content,
					Link = "/history/order",
					DateCreated = DateTime.Now,
					IsReaded = false,
				};

				if (connections != null)
				{
					foreach (var connection in connections)
					{
						await _hubContext.Clients.Clients(connection)
							.SendAsync(Constants.SIGNAL_R_NOTIFICATION_HUB_RECEIVE_NOTIFICATION,
							JsonConvert.SerializeObject(_mapper.Map<NotificationRespone>(notification)));
					}
				}
				
				_notificationRepositiory.AddNotification(notification);
				return Ok();
			}
			catch
			{
				return StatusCode(500);
			}
		}
        #endregion

        #region Edit notification isreaded
        [Authorize]
        [HttpPut("editNotificationIsReaded/{id}")]
        public IActionResult EditNotificationIsReaded(int id)
        {
            try
            {
                Notification? notification = _notificationRepositiory.GetNotificationById(id);
                if (notification == null) return Conflict();
                _notificationRepositiory.EditNotificationIsReaded(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
		# endregion

        #region Edit read all notifications
        [Authorize]
        [HttpPut("editReadAllNotifications/{id}")]
        public IActionResult EditReadAllNotifications(int id)
        {
            try
            {
                _notificationRepositiory.EditReadAllNotifications(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion

        #region Fetch More Notifications
        [Authorize]
        [HttpPost("fetchMoreNotifications")]
        public IActionResult FetchMoreNotifications([FromBody] GetAllOrderRequestDTO request)
        {
            try
			{
				var notifications = _notificationRepositiory.GetNotifications(request.UserId, request.Offset);
                List<NotificationRespone> responseData = new List<NotificationRespone>();
				foreach (var notification in notifications)
				{
                    NotificationRespone notif = new NotificationRespone();
					notif.NotificationId = notification.NotificationId;
					notif.Title = notification.Title;
					notif.Content = notification.Content;
					notif.Link = notification.Link;
					notif.DateCreated = notification.DateCreated;
					notif.IsReaded = notification.IsReaded;
					responseData.Add(notif);

                }
                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, JsonConvert.SerializeObject(_mapper.Map<List<NotificationRespone>>(responseData))));
            }
			catch (Exception e)
			{
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
        #endregion
    }
}
