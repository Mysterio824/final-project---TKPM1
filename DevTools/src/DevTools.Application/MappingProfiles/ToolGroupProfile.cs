using AutoMapper;
using DevTools.Application.DTOs.Request.ToolGroup;
using DevTools.Application.DTOs.Response.ToolGroup;
using DevTools.Domain.Entities;

namespace DevTools.Application.MappingProfiles;

public class ToolGroupProfile : Profile
{
    public ToolGroupProfile()
    {
        CreateMap<CreateToolGroupDto, ToolGroup>();

        CreateMap<UpdateToolGroupDto, ToolGroup>();

        CreateMap<ToolGroup, ToolGroupDto>();
    }
}
