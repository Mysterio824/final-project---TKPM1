using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.Application.DTOs.Response.User
{
    public class RefreshTokenResponseDto(
        string newAccessToken, 
        string newRefreshToken)
    {
        public string AccessToken { get; set; } = newAccessToken;
        public string RefreshToken { get; set; } = newRefreshToken;
    }
}
