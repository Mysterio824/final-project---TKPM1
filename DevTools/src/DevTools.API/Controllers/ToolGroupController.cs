using System.Security.Claims;
using DevTools.Application.DTOs.Request.ToolGroup;
using DevTools.Application.DTOs.Response;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Application.DTOs.Response.ToolGroup;
using DevTools.Application.Services;
using DevTools.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DevTools.API.Controllers
{
    public class ToolGroupController(
        IToolGroupService toolGroupService,
        IToolQueryService toolService) : ApiController
    {
        private readonly IToolQueryService _toolService = toolService;
        private readonly IToolGroupService _toolGroupService = toolGroupService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(ApiResult<IEnumerable<ToolGroupResponseDto>>.Success(await _toolGroupService.GetAllAsync()));
        }

        [HttpGet("{id}/todoItems")]
        public async Task<IActionResult> GetAllToolItemsAsync(int id)
        {
            var userRole =  GetUserRole();
            var userId =  GetUserId();

            return Ok(ApiResult<IEnumerable<ToolItemResponseDto>>.Success(
                await _toolService.GetToolByGroupIdAsync(id, userRole, userId)));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateToolGroupDto request)
        {
            return Ok(ApiResult<CreateToolGroupResponseDto>.Success(
                await _toolGroupService.CreateAsync(request)));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAsync(UpdateToolGroupDto request)
        {
            return Ok(ApiResult<UpdateToolGroupResponseDto>.Success(
                await _toolGroupService.UpdateAsync(request)));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            return Ok(ApiResult<BaseResponseDto>.Success(await _toolGroupService.DeleteAsync(id)));
        }

        private UserRole GetUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleClaim))
            {
                return UserRole.Anonymous;
            }
            return Enum.Parse<UserRole>(roleClaim, true);
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userIdClaim) ? -1 : int.Parse(userIdClaim);
        }

    }
}
