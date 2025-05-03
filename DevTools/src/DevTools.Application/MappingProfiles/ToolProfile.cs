using AutoMapper;
using DevTools.Domain.Entities;
using DevTools.Domain.Enums;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Application.DTOs.Request.Tool;
using DevTools.Application.DTOs.Response;

namespace DevTools.Application.MappingProfiles
{
    public class ToolProfile : Profile
    {
        public ToolProfile()
        {
            CreateMap<CreateToolDto, Tool>()
                .ForMember(dest => dest.DllPath, opt => opt.Ignore());

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

            CreateMap<Tool, BaseResponseDto>();
        }

        private static bool IsToolAccessible(Tool tool, UserRole role)
        {
            return tool.IsEnabled;
        }
    }
}
