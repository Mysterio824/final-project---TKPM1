using DevTools.DTOs.Request;
using DevTools.Entities;
using DevTools.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevTools.Controllers
{
    [Route("api/tools")]
    [ApiController]
    public class ToolController : ControllerBase
    {
        private readonly IToolService _toolService;

        public ToolController(IToolService toolService)
        {
            _toolService = toolService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Tool>>> GetTools()
        {
            await _toolService.UpdateToolList();
            return Ok(await _toolService.GetToolsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tool>> GetToolById(int id)
        {
            var tool = await _toolService.GetToolByIdAsync(id);
            if (tool == null)
                return NotFound();

            return Ok(tool);
        }

        [HttpPost("execute/{id}")]
        public ActionResult<string> ExecuteTool(int id, [FromBody] ToolRequest request)
        {
            try
            {
                return Ok(new { result = _toolService.ExecuteTool(id, request.inputString) });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
