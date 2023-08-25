using AutoMapper;
using BusinessObject;
using DTOs;
using RealTimeServerAPI.DTOs;
using NotificationRespone = RealTimeServerAPI.DTOs.NotificationRespone;

namespace RealTimeServerAPI
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<Notification, NotificationRespone>().ReverseMap();
		}
	}
}
