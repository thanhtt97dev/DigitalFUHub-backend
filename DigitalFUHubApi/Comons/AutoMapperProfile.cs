using AutoMapper;
using DataAccess.DAOs;
using DTOs;
using DigitalFUHubApi.Comons;
using BusinessObject.Entities;
using DTOs.Bank;
using DTOs.User;
using DTOs.Notification;
using DTOs.Chat;
using DTOs.Product;
using DTOs.Tag;

namespace DigitalFUHubApi.Comons
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserResponeDTO>()
                .ForMember(des => des.RoleName, act => act.MapFrom(src => src.Role.RoleName)).ReverseMap();
			CreateMap<User, UserSignInResponseDTO>()
				.ForMember(des => des.RoleName, act => act.MapFrom(src => src.Role.RoleName)).ReverseMap();
			CreateMap<User, UserUpdateRequestDTO>().ReverseMap();
            CreateMap<Role, RoleDTO>().ReverseMap();
            CreateMap<Notification, NotificationRespone>().ReverseMap();
			CreateMap<DepositTransaction, CreateTransactionRequestDTO>().ReverseMap();
			CreateMap<WithdrawTransaction, CreateTransactionRequestDTO>().ReverseMap();
			CreateMap<Bank, BankResponeDTO>().ReverseMap();
			CreateMap<UserBank, BankAccountResponeDTO>()
				.ForMember(des => des.BankName, act => act.MapFrom(src => src.Bank.BankName))
				.ForMember(des => des.CreditAccount, act => act.MapFrom(src => Util.HideCharacters(src.CreditAccount,5)))
				.ReverseMap();
            CreateMap<Message, MessageResponseDTO>().ReverseMap();
			CreateMap<WithdrawTransactionBill, WithdrawTransactionBillDTO>().ReverseMap();
			CreateMap<WithdrawTransaction, HistoryWithdrawResponsetDTO>()
				.ForMember(des => des.BankName, act => act.MapFrom(src => src.UserBank.Bank.BankName))
				.ForMember(des => des.CreditAccountName, act => act.MapFrom(src => src.UserBank.CreditAccountName))
				.ForMember(des => des.CreditAccount, atc => atc.MapFrom(src => Util.HideCharacters(src.UserBank.CreditAccount, 5)))
				.ReverseMap();

			CreateMap<Product, ProductResponseDTO>().ReverseMap(); ;
			CreateMap<ProductVariant, ProductVariantResponseDTO>().ReverseMap(); ;
			CreateMap<ProductMedia, ProductMediaResponseDTO>().ReverseMap(); ;
			CreateMap<Tag, TagResponseDTO>().ReverseMap(); ;
			CreateMap<AssetInformation, AssetInformationResponseDTO>().ReverseMap();
				
		}
    }
}
