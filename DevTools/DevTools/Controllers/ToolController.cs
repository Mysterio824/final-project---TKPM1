using DevTools.DTOs.Request;
using DevTools.DTOs.Response;
using DevTools.Entities;
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
    public class ToolController : ControllerBase
    {
        private readonly IToolService _toolService;
        private readonly ToolActionStrategyFactory _strategyFactory;

        public ToolController(
            IToolService toolService, 
            ToolActionStrategyFactory strategyFactory
        ){
            _toolService = toolService;
            _strategyFactory = strategyFactory;
        }

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
        public async Task<ActionResult> AddTool([FromForm] IFormFile tool)
        {
            try
            {
                await _toolService.AddToolAsync(tool);
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
