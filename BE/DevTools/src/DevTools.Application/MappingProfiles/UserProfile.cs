using AutoMapper;
using DevTools.Application.DTOs.Response.User;
using DevTools.Domain.Entities;

namespace DevTools.Application.MappingProfiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore());
    }
}