using DevTools.Application.DTOs.Response;
using DevTools.Application.Services;
using DevTools.Application.Strategies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevTools.Application.DTOs.Request.Tool;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Domain.Enums;
using System.Security.Claims;

namespace DevTools.API.Controllers
{
    public class ToolController(
        IToolQueryService toolQueryService,
        IToolCommandService toolCommandService,
        ToolActionStrategyFactory strategyFactory,
        ILogger<ToolController> logger) : ApiController
    {
        private readonly IToolQueryService _toolQueryService = toolQueryService ?? throw new ArgumentNullException(nameof(toolQueryService));
        private readonly IToolCommandService _toolCommandService = toolCommandService ?? throw new ArgumentNullException(nameof(toolCommandService));
        private readonly ToolActionStrategyFactory _strategyFactory = strategyFactory ?? throw new ArgumentNullException(nameof(strategyFactory));
        private readonly ILogger<ToolController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpGet("all")]
        public async Task<IActionResult> GetTools()
        {
            var userRole =  GetUserRole();
            var userId =  GetUserId();

            //await _toolCommandService.UpdateToolList();
            var tools = await _toolQueryService.GetToolsAsync(userRole, userId);
            return Ok(ApiResult<IEnumerable<ToolItemResponseDto>>.Success(tools));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetToolById(int id)
        {
            var userRole =  GetUserRole();
            var userId =  GetUserId();

            var result = await _toolQueryService.GetToolByIdAsync(id, userRole, userId);
            return Ok(ApiResult<ToolResponseDto>.Success(result));
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResult<IEnumerable<ToolItemResponseDto>>>> GetToolsByName([FromQuery] string name)
        {
            var userRole =  GetUserRole();
            var userId =  GetUserId();

            await _toolCommandService.UpdateToolList();
            var tools = await _toolQueryService.GetToolsByNameAsync(name, userRole, userId);
            return Ok( ApiResult<IEnumerable<ToolItemResponseDto>>.Success(tools) );
        }

        [Authorize]
        [HttpGet("favorite/all")]
        public async Task<IActionResult> GetFavoriteTools()
        {
            var userRole =  GetUserRole();
            var userId =  GetUserId();

            await _toolCommandService.UpdateToolList();
            var favoriteTools = await _toolQueryService.GetToolFavoriteAsync(userRole, userId);

            return Ok( ApiResult<IEnumerable<ToolItemResponseDto>>.Success(favoriteTools) );
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddTool(CreateToolDto request)
        {
            return Ok(ApiResult<CreateToolResponseDto>
                .Success(await _toolCommandService.AddToolAsync(request)));
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/{actionName}")]
        public async Task<IActionResult> UpdateTool(int id, string actionName)
        {
            var strategy = _strategyFactory.GetStrategy(actionName);
            var result = await strategy.ExecuteAsync(id);
            return Ok(ApiResult<BaseResponseDto>.Success(result));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTool(int id)
        {
            return Ok(ApiResult<BaseResponseDto>.Success(await _toolCommandService.DeleteToolAsync(id)));
        }

        private UserRole GetUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleClaim))
            {
                _logger.LogWarning("No role claim found for user.");
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