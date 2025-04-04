using DevTools.Application.DTOs.Request;
using DevTools.Application.DTOs.Response;
using DevTools.Application.Services;
using DevTools.Domain.Enums;
using DevTools.Application.Strategies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevTools.API.Controllers
{
    public class ToolController(
        IToolQueryService toolQueryService,
        IToolCommandService toolCommandService,
        IToolExecutionService toolExecutionService,
        ToolActionStrategyFactory strategyFactory,
        ILogger<ToolController> logger) : ApiController
    {
        private readonly IToolQueryService _toolQueryService = toolQueryService ?? throw new ArgumentNullException(nameof(toolQueryService));
        private readonly IToolCommandService _toolCommandService = toolCommandService ?? throw new ArgumentNullException(nameof(toolCommandService));
        private readonly IToolExecutionService _toolExecutionService = toolExecutionService ?? throw new ArgumentNullException(nameof(toolExecutionService));
        private readonly ToolActionStrategyFactory _strategyFactory = strategyFactory ?? throw new ArgumentNullException(nameof(strategyFactory));
        private readonly ILogger<ToolController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpGet("all")]
        public async Task<ActionResult<ApiResult<IEnumerable<ToolDto>>>> GetTools()
        {
            var userRole = GetUserRole();
            var userId = GetUserId();

            await _toolCommandService.UpdateToolList();
            var tools = await _toolQueryService.GetToolsAsync(userRole, userId);
            return Ok(ApiResult<IEnumerable<ToolDto>>.Success(tools));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<ToolDto>>> GetToolById(int id)
        {
            var userRole = GetUserRole();
            var userId = GetUserId();

            var tool = await _toolQueryService.GetToolByIdAsync(id, userRole, userId);
            if (tool == null)
                return NotFound();

            return Ok(ApiResult<ToolDto>.Success(tool));
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResult<IEnumerable<ToolDto>>>> GetToolsByName([FromQuery] string name)
        {
            var userRole = GetUserRole();
            var userId = GetUserId();

            await _toolCommandService.UpdateToolList();
            var tools = await _toolQueryService.GetToolsByNameAsync(name, userRole, userId);
            return Ok( ApiResult<IEnumerable<ToolDto>>.Success(tools) );
        }

        [Authorize]
        [HttpGet("favorite/all")]
        public async Task<ActionResult<ApiResult<IEnumerable<ToolDto>>>> GetFavoriteTools()
        {
            var userRole = GetUserRole();
            var userId = GetUserId();

            await _toolCommandService.UpdateToolList();
            var favoriteTools = await _toolQueryService.GetToolFavoriteAsync(userRole, userId);

            return Ok( ApiResult<IEnumerable<ToolDto>>.Success(favoriteTools) );
        }

        [HttpPost("execute")]
        public async Task<ActionResult<ApiResult<ToolResponseDto>>> ExecuteTool([FromForm] ToolRequest request)
        {
            var userRole = GetUserRole();

            return Ok(ApiResult<ToolResponseDto>
                .Success(await _toolExecutionService
                                .ExecuteToolAsync(request.ToolId, 
                                                  request.InputText, 
                                                  request.UploadedFile, 
                                                  userRole
                                                 )
                                )
                );
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> AddTool(IFormFile tool)
        {
            if (tool == null)
            {
                _logger.LogError("No file received.");
                return BadRequest(new { Message = "No file uploaded." });
            }

            await _toolCommandService.AddToolAsync(tool);
            _logger.LogInformation("Tool {ToolName} added successfully.", tool.FileName);
            return Ok(ApiResult<String>.Success($"Tool {tool.FileName} added successfully."));
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/{actionName}")]
        public async Task<ActionResult<string>> UpdateTool(int id, string actionName)
        {
            var strategy = _strategyFactory.GetStrategy(actionName);
            var result = await strategy.ExecuteAsync(id);
            _logger.LogInformation("Tool {ToolId} updated with action {ActionName}.", id, actionName);
            return Ok(ApiResult<String>.Success(result));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTool(int id)
        {
            await _toolCommandService.DeleteToolAsync(id);
            _logger.LogInformation("Tool {ToolId} deleted successfully.", id);
            return Ok(ApiResult<String>.Success($"Tool {id} deleted successfully."));
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