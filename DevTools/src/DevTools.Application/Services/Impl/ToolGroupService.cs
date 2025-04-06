using AutoMapper;
using DevTools.Application.DTOs.Request.ToolGroup;
using DevTools.Application.DTOs.Response;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Application.DTOs.Response.ToolGroup;
using DevTools.Application.Exceptions;
using DevTools.Domain.Entities;
using DevTools.Infrastructure.Repositories;
using DevTools.Infrastructure.Repositories.impl;

namespace DevTools.Application.Services.Impl
{
    public class ToolGroupService(
        IToolGroupRepository toolGroupRepository,
        IMapper mapper
        ) : IToolGroupService
    {
        private readonly IToolGroupRepository _toolGroupRepository = toolGroupRepository ?? throw new ArgumentNullException(nameof(toolGroupRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        public async Task<CreateToolGroupResponseDto> CreateAsync(CreateToolGroupDto request)
        {
            if (request == null)
            {
                throw new BadRequestException("Tool group request cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Tool group name cannot be null or empty.");
            }

            var toolGroup = _mapper.Map<ToolGroup>(request);
            var result = await _toolGroupRepository.AddAsync(toolGroup);
            return _mapper.Map<CreateToolGroupResponseDto>(result);
        }

        public async Task<BaseResponseDto> DeleteAsync(int id)
        {
            var toolGroup = await _toolGroupRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Tool group with id {id} not found.");
            var result = await _toolGroupRepository.DeleteAsync(toolGroup);
            return _mapper.Map<BaseResponseDto>(result);

        }

        public async Task<IEnumerable<ToolGroupResponseDto>> GetAllAsync()
        {
            var result = await _toolGroupRepository.GetAllAsync(x => true);
            return _mapper.Map<IEnumerable<ToolGroupResponseDto>>(result);
        }

        public async Task<UpdateToolGroupResponseDto> UpdateAsync(UpdateToolGroupDto request)
        {
            var toolGroup = await _toolGroupRepository.GetByIdAsync(request.Id)
                ?? throw new NotFoundException($"Tool group with id {request.Id} not found.");
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Tool group name cannot be null or empty.");
            }
            toolGroup = _mapper.Map<ToolGroup>(request);
            var result = await _toolGroupRepository.UpdateAsync(toolGroup);
            return _mapper.Map<UpdateToolGroupResponseDto>(result);
        }
    }
}
