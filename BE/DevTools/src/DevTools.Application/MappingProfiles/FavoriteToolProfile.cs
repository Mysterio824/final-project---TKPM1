using AutoMapper;
using DevTools.Application.DTOs.Response;
using DevTools.Domain.Entities;

namespace DevTools.Application.MappingProfiles
{
    public class FavoriteToolProfile : Profile
    {
        public FavoriteToolProfile()
        {
            CreateMap<FavoriteTool, BaseResponseDto>();
        }

    }
}
