using AutoMapper;
using BusinessObject;
using DTOs;

namespace FuMarketAPI.Comons
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserResponeDTO>().ForMember(des => des.RoleName, act => act.MapFrom(src => src.Role.RoleName)).ReverseMap();
            CreateMap<User, UserUpdateRequestDTO>().ReverseMap();
            CreateMap<Role, RoleDTO>().ReverseMap();
            CreateMap<Notification, NotificationRespone>().ReverseMap();
        }
    }
}
