using DevTools.Application.DTOs.Request;
using DevTools.Application.DTOs.Response;
using DevTools.Application.Interfaces.Services;
using DevTools.Domain.Enums;
using DevTools.Domain.Exceptions;
using DevTools.Infrastructure.Strategies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevTools.API.Controllers
{
    [Route("api/tools")]
    [ApiController]
    public class ToolController(
        IToolQueryService toolQueryService,
        IToolCommandService toolCommandService,
        IToolExecutionService toolExecutionService,
        ToolActionStrategyFactory strategyFactory,
        ILogger<ToolController> logger) : ControllerBase
    {
        private readonly IToolQueryService _toolQueryService = toolQueryService ?? throw new ArgumentNullException(nameof(toolQueryService));
        private readonly IToolCommandService _toolCommandService = toolCommandService ?? throw new ArgumentNullException(nameof(toolCommandService));
        private readonly IToolExecutionService _toolExecutionService = toolExecutionService ?? throw new ArgumentNullException(nameof(toolExecutionService));
        private readonly ToolActionStrategyFactory _strategyFactory = strategyFactory ?? throw new ArgumentNullException(nameof(strategyFactory));
        private readonly ILogger<ToolController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ToolDTO>>> GetTools()
        {
            var userRole = GetUserRole();
            var userId = GetUserId();

            await _toolCommandService.UpdateToolList();
            var tools = await _toolQueryService.GetToolsAsync(userRole, userId);
            return Ok(tools);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ToolDTO>> GetToolById(int id)
        {
            var userRole = GetUserRole();
            var userId = GetUserId();

            var tool = await _toolQueryService.GetToolByIdAsync(id, userRole, userId);
            if (tool == null)
                return NotFound();

            return Ok(tool);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ToolDTO>>> GetToolsByName([FromQuery] string name)
        {
            var userRole = GetUserRole();
            var userId = GetUserId();

            await _toolCommandService.UpdateToolList();
            var tools = await _toolQueryService.GetToolsByNameAsync(name, userRole, userId);
            return Ok(tools);
        }

        [Authorize]
        [HttpGet("favorite/all")]
        public async Task<ActionResult<IEnumerable<ToolDTO>>> GetFavoriteTools()
        {
            var userRole = GetUserRole();
            var userId = GetUserId();

            await _toolCommandService.UpdateToolList();
            var favoriteTools = await _toolQueryService.GetToolFavoriteAsync(userRole, userId);
            return Ok(favoriteTools);
        }

        [HttpPost("execute")]
        public async Task<ActionResult<ToolResponse>> ExecuteTool([FromForm] ToolRequest request)
        {
            var userRole = GetUserRole();

            try
            {
                var result = await _toolExecutionService.ExecuteToolAsync(request.ToolId, request.InputText, request.UploadedFile, userRole);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid tool execution request.");
                return BadRequest(new { ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized tool execution attempt.");
                return Unauthorized(new { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool with ID {ToolId}.", request.ToolId);
                return StatusCode(500, new { Message = "Failed to execute tool", Detail = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> AddTool(IFormFile tool)
        {
            try
            {
                if (tool == null)
                {
                    _logger.LogError("No file received.");
                    return BadRequest(new { Message = "No file uploaded." });
                }

                await _toolCommandService.AddToolAsync(tool);
                _logger.LogInformation("Tool {ToolName} added successfully.", tool.FileName);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid tool file.");
                return BadRequest(new { ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Tool addition failed due to validation.");
                return BadRequest(new { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding tool.");
                return StatusCode(500, new { Message = "Failed to add tool", Detail = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/{actionName}")]
        public async Task<ActionResult<string>> UpdateTool(int id, string actionName)
        {
            try
            {
                var strategy = _strategyFactory.GetStrategy(actionName);
                var result = await strategy.ExecuteAsync(id);
                _logger.LogInformation("Tool {ToolId} updated with action {ActionName}.", id, actionName);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Tool {ToolId} not found for action {ActionName}.", id, actionName);
                return NotFound(new { ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on tool {ToolId} for action {ActionName}.", id, actionName);
                return BadRequest(new { ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid action {ActionName} for tool {ToolId}.", actionName, id);
                return BadRequest(new { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tool {ToolId} with action {ActionName}.", id, actionName);
                return StatusCode(500, new { Message = $"Failed to {actionName.ToLower()} tool", Detail = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTool(int id)
        {
            try
            {
                await _toolCommandService.DeleteToolAsync(id);
                _logger.LogInformation("Tool {ToolId} deleted successfully.", id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tool {ToolId}.", id);
                return StatusCode(500, new { Message = "Failed to delete tool", Detail = ex.Message });
            }
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