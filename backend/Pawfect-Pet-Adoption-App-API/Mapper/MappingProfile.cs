using AutoMapper;
using Pawfect_Pet_Adoption_App_API.DTOs.User;
using Pawfect_Pet_Adoption_App_API.Models;

namespace Pawfect_Pet_Adoption_App_API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, GUserDTO>().ReverseMap();
            CreateMap<CUserDTO, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<User, CUserDTO>();
            CreateMap<UUserDTO, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<User, UUserDTO>();
        }
    }
}