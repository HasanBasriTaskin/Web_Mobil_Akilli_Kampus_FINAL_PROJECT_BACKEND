using AutoMapper;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;
using SMARTCAMPUS.EntityLayer.DTOs.User;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, RegisterDto>().ReverseMap();
            
            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.IdString, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Roles logic handles separately usually, or via custom resolver
        }
    }
}
