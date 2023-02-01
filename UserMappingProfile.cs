using AutoMapper;
using PaczkomatDatabaseAPI.Models;

namespace PaczkomatDatabaseAPI
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile() 
        {
            CreateMap<CreateUserDto, User>()
                .ForMember(user => user.AccountType, opt => opt.MapFrom((src, user, destMember, context) => context.Items["AccountType"]));
        }
    }
}
