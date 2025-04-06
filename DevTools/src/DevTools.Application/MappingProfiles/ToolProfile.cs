using AutoMapper;
using DevTools.Domain.Entities;
using DevTools.Domain.Enums;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Application.DTOs.Request.Tool;

namespace DevTools.Application.MappingProfiles
{
    public class ToolProfile : Profile
    {
        public ToolProfile()
        {
            CreateMap<CreateToolDto, Tool>();

            CreateMap<UpdateToolDto, Tool>();

            CreateMap<Tool, ToolItemResponseDto>()
                .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom((src, dest, destMember, context) =>
                    IsToolAccessible(src, (UserRole)context.Items["UserRole"])))
                .ForMember(dest => dest.IsFavorite, opt => opt.Ignore());

            CreateMap<Tool, ToolResponseDto>()
                .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom((src, dest, destMember, context) =>
                    IsToolAccessible(src, (UserRole)context.Items["UserRole"])))
                .ForMember(dest => dest.IsFavorite, opt => opt.Ignore())
                .ForMember(dest => dest.File, opt => opt.Ignore());
        }

        private static bool IsToolAccessible(Tool tool, UserRole role)
        {
            return !tool.IsPremium || (role != UserRole.User && role != UserRole.Anonymous && tool.IsEnabled);
        }
    }
}
