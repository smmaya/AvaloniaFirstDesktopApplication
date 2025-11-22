using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Shared.Interfaces;

namespace Avalonia.ToDo.Desktop.Helpers;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IAuthService _authService;

    public AuthHeaderHandler(IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Wait until token exists (if you want automatic refresh, add logic here)
        var token = _authService.Token;

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            // Optionally throw or log that the request is unauthenticated
            Console.WriteLine("Warning: sending request without JWT token!");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}