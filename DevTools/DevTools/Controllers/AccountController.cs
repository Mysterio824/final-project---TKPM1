using Microsoft.AspNetCore.Mvc;
using DevTools.Interfaces.Services;
using System.Diagnostics;
using DevTools.Exceptions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace DevTools.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [Authorize]
        [HttpPost("favorite/{id}/add")]
        public async Task<ActionResult> AddFavoriteTool(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                await _accountService.AddFavoriteToolAsync(userId, id);
                return Ok();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(e.Message);
            }
        }

        [Authorize]
        [HttpPost("favorite/{id}/remove")]
        public async Task<ActionResult> RemoveFavoriteTool(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                await _accountService.RemoveFavoriteToolAsync(userId, id);
                return Ok();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(e.Message);
            }
        }

        [Authorize]
        [HttpPost("premium/request")]
        public async Task<ActionResult> SendPremiumRequest()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                await _accountService.SendPremiumRequestAsync(userId);
                return Ok();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(e.Message);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpPost("premium/revoke")]
        public async Task<ActionResult> SendRevokePremiumRequest()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            try
            {
                await _accountService.SendRevokePremiumRequestAsync(userId);
                return Ok();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(e.Message);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
