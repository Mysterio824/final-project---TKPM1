using Microsoft.AspNetCore.Mvc;
using DevTools.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace DevTools.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController(
        IAccountService accountService
            ) : Controller
    {
        private readonly IAccountService _accountService = accountService;

        [Authorize]
        [HttpPost("favorite/{id}/add")]
        public async Task<ActionResult> AddFavoriteTool(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _accountService.AddFavoriteToolAsync(userId, id);
            return Ok();
        }

        [Authorize]
        [HttpPost("favorite/{id}/remove")]
        public async Task<ActionResult> RemoveFavoriteTool(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _accountService.RemoveFavoriteToolAsync(userId, id);
            return Ok();
        }

        [Authorize]
        [HttpPost("premium/request")]
        public async Task<ActionResult> SendPremiumRequest()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _accountService.SendPremiumRequestAsync(userId);
            return Ok();
        }

        [Authorize]
        [HttpPost("premium/revoke")]
        public async Task<ActionResult> SendRevokePremiumRequest()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _accountService.SendRevokePremiumRequestAsync(userId);
            return Ok();
        }
    }
}
