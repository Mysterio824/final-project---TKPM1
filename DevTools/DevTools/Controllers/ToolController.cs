using DevTools.DTOs.Request;
using DevTools.DTOs.Response;
using DevTools.Enums;
using DevTools.Exceptions;
using DevTools.Interfaces.Services;
using DevTools.Strategies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevTools.Controllers
{
    [Route("api/tools")]
    [ApiController]
    public class ToolController(
        IToolService toolService,
        ToolActionStrategyFactory strategyFactory,
        ILogger<ToolController> logger
        ) : ControllerBase
    {
        private readonly IToolService _toolService = toolService;
        private readonly ToolActionStrategyFactory _strategyFactory = strategyFactory;
        private readonly ILogger<ToolController> _logger = logger;

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ToolDTO>>> GetTools()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)!.Value;
            var userRole = Enum.Parse<UserRole>(roleClaim, true);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _toolService.UpdateToolList();
            return Ok(await _toolService.GetToolsAsync(userRole, userId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ToolDTO>> GetToolById(int id)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)!.Value;
            var userRole = Enum.Parse<UserRole>(roleClaim, true);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var tool = await _toolService.GetToolByIdAsync(id, userRole, userId);
            if (tool == null)
                return NotFound();
            if(tool.IsPremium && (userRole == UserRole.User || userRole == UserRole.Anonymous))
                return Unauthorized();

            return Ok(tool);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ToolDTO>>> GetToolsByName([FromQuery] string name)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)!.Value;
            var userRole = Enum.Parse<UserRole>(roleClaim, true);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _toolService.UpdateToolList();
            return Ok(await _toolService.GetToolsByNameAsync(name, userRole, userId));
        }

        [Authorize]
        [HttpGet("favorite/all")]
        public async Task<ActionResult<IEnumerable<ToolDTO>>> GetFavoriteTools()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)!.Value;
            var userRole = Enum.Parse<UserRole>(roleClaim, true);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _toolService.UpdateToolList();
            return Ok(await _toolService.GetToolFavoriteAsync(userRole, userId));
        }

        [HttpPost("execute")]
        public async Task<ActionResult<ToolResponse>> ExecuteTool([FromForm] ToolRequest request)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)!.Value;
            var userRole = Enum.Parse<UserRole>(roleClaim, true);

            try
            {
                var result = await _toolService.ExecuteToolAsync(request.ToolId, request.InputText, request.UploadedFile, userRole);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
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

                if (Path.GetExtension(tool.FileName)?.ToLower() != ".dll")
                {
                    return BadRequest(new { Message = "Invalid file type. Only DLL files are allowed." });
                }

                await _toolService.AddToolAsync(tool);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error in AddTool.");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding the tool.");
                return StatusCode(500, new { Message = ex.Message });
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
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Failed to {actionName.ToLower()} tool", Detail = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTool(int id)
        {
            try
            {
                await _toolService.DeleteToolAsync(id);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
