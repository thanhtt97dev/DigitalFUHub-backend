using AutoMapper;
using BusinessObject;
using DTOs;
using RealTimeServerAPI.DTOs;

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
