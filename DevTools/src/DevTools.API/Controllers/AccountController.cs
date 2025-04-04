using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DevTools.Application.Exceptions;
using DevTools.Application.Services;
using DevTools.Application.DTOs.Response;

namespace DevTools.API.Controllers
{
    [Authorize]
    public class AccountController(
        IFavoriteToolService favoriteToolService,
        IPremiumService premiumService) : ApiController
    {
        private readonly IFavoriteToolService _favoriteToolService = favoriteToolService ?? throw new ArgumentNullException(nameof(favoriteToolService));
        private readonly IPremiumService _premiumService = premiumService ?? throw new ArgumentNullException(nameof(premiumService));

        [HttpPost("favorite/{id}/add")]
        public async Task<ActionResult> AddFavoriteTool(int id)
        {
            var userId = GetUserId();
            await _favoriteToolService.AddFavoriteToolAsync(userId, id);
            return Ok(ApiResult<String>.Success("Add successfully"));
        }

        [HttpPost("favorite/{id}/remove")]
        public async Task<ActionResult> RemoveFavoriteTool(int id)
        {
            var userId = GetUserId();
            await _favoriteToolService.RemoveFavoriteToolAsync(userId, id);
            return Ok(ApiResult<String>.Success("Remove successfully"));
        }

        [HttpPost("premium/request")]
        public async Task<ActionResult> SendPremiumRequest()
        {
            var userId = GetUserId();
            await _premiumService.SendPremiumRequestAsync(userId);
            return Ok(ApiResult<String>.Success("Premium request sent"));
        }

        [HttpPost("premium/revoke")]
        public async Task<ActionResult> SendRevokePremiumRequest()
        {
            var userId = GetUserId();
            await _premiumService.SendRevokePremiumRequestAsync(userId);
            return Ok(ApiResult<String>.Success("Premium revoke request sent"));
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedException("User ID not found in token");

            return int.Parse(userIdClaim);
        }
    }
}