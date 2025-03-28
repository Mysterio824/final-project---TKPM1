using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DevTools.Application.Interfaces.Services;
using DevTools.Domain.Exceptions;

namespace DevTools.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController(
        IFavoriteToolService favoriteToolService,
        IPremiumService premiumService) : ControllerBase
    {
        private readonly IFavoriteToolService _favoriteToolService = favoriteToolService ?? throw new ArgumentNullException(nameof(favoriteToolService));
        private readonly IPremiumService _premiumService = premiumService ?? throw new ArgumentNullException(nameof(premiumService));

        [Authorize]
        [HttpPost("favorite/{id}/add")]
        public async Task<ActionResult> AddFavoriteTool(int id)
        {
            try
            {
                var userId = GetUserId();
                await _favoriteToolService.AddFavoriteToolAsync(userId, id);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to add favorite tool", Detail = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("favorite/{id}/remove")]
        public async Task<ActionResult> RemoveFavoriteTool(int id)
        {
            try
            {
                var userId = GetUserId();
                await _favoriteToolService.RemoveFavoriteToolAsync(userId, id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to remove favorite tool", Detail = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("premium/request")]
        public async Task<ActionResult> SendPremiumRequest()
        {
            try
            {
                var userId = GetUserId();
                await _premiumService.SendPremiumRequestAsync(userId);
                return Ok(new { Message = "Premium request sent" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to request premium status", Detail = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("premium/revoke")]
        public async Task<ActionResult> SendRevokePremiumRequest()
        {
            try
            {
                var userId = GetUserId();
                await _premiumService.SendRevokePremiumRequestAsync(userId);
                return Ok(new { Message = "Premium revoke request sent" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to revoke premium status", Detail = ex.Message });
            }
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