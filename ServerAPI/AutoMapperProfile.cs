using AutoMapper;
using BusinessObject;
using DTOs;

namespace ServerAPI
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<User, UserResponeDTO>().ForMember(des => des.RoleName, act => act.MapFrom(src => src.Role.RoleName)).ReverseMap();
			CreateMap<User, UserRequestDTO>().ReverseMap();
			CreateMap<Role, RoleDTO>().ReverseMap();
		}
	}
}
