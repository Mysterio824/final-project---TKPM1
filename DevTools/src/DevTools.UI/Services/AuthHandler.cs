using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public class AuthHandler : DelegatingHandler
    {
        private readonly ICurrentUserService _currentUserService;

        public AuthHandler(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_currentUserService.CurrentUser?.Token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentUserService.CurrentUser.Token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
