using System;
using System.Net.Mime;
using System.Reflection;
using Jellyfin.Plugin.Jellio.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.Jellio.Controllers;

[ApiController]
[Route("jellio")]
public class WebController : ControllerBase
{
    private readonly IUserManager _userManager;
    private readonly IUserViewManager _userViewManager;
    private readonly IDtoService _dtoService;
    private readonly IServerApplicationHost _serverApplicationHost;
    private readonly IDeviceManager _deviceManager;
    private readonly Assembly _executingAssembly = Assembly.GetExecutingAssembly();

    public WebController(
        IUserManager userManager,
        IUserViewManager userViewManager,
        IDtoService dtoService,
        IServerApplicationHost serverApplicationHost,
        IDeviceManager deviceManager
    )
    {
        _userManager = userManager;
        _userViewManager = userViewManager;
        _dtoService = dtoService;
        _serverApplicationHost = serverApplicationHost;
        _deviceManager = deviceManager;
    }

    [HttpGet]
    [HttpGet("configure")]
    [HttpGet("{config?}/configure")]
    public IActionResult GetIndex(string? config = null)
    {
        const string ResourceName = "Jellyfin.Plugin.Jellio.Web.index.html";

        var resourceStream = _executingAssembly.GetManifestResourceStream(ResourceName);

        if (resourceStream == null)
        {
            return NotFound($"Resource {ResourceName} not found.");
        }

        return new FileStreamResult(resourceStream, "text/html");
    }

    [HttpGet("server-info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(MediaTypeNames.Application.Json)]
    public IActionResult GetServerInfo()
    {
        // Try claims principal first (cookie/session auth)
        var userId = RequestHelpers.GetCurrentUserId(User);

        // Fallback: try token-based auth from headers
        if (userId == null)
        {
            var token = ExtractTokenFromHeaders(Request);
            if (!string.IsNullOrEmpty(token))
            {
                userId = RequestHelpers.GetUserIdByAuthToken(token!, _deviceManager);
            }
        }

        if (userId == null)
        {
            return Unauthorized();
        }

        var friendlyName = _serverApplicationHost.FriendlyName;
        var libraries = LibraryHelper.GetUserLibraries(userId.Value, _userManager, _userViewManager, _dtoService);

        return Ok(new { name = friendlyName, libraries });
    }

    private static string? ExtractTokenFromHeaders(HttpRequest request)
    {
        // Priority: X-Emby-Token
        if (request.Headers.TryGetValue("X-Emby-Token", out var embyToken) && !string.IsNullOrWhiteSpace(embyToken))
        {
            return embyToken.ToString();
        }

        // Try Authorization: MediaBrowser Token="..."
        if (request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var auth = authHeader.ToString();
            var token = ParseTokenFromMediaBrowserHeader(auth);
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }
        }

        // Try X-Emby-Authorization: MediaBrowser Client=..., Token="..."
        if (request.Headers.TryGetValue("X-Emby-Authorization", out var embyAuthHeader))
        {
            var token = ParseTokenFromMediaBrowserHeader(embyAuthHeader.ToString());
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }
        }

        return null;
    }

    private static string? ParseTokenFromMediaBrowserHeader(string headerValue)
    {
        // Expect formats like:
        // MediaBrowser Token="..."
        // MediaBrowser Client="...", Device="...", DeviceId="...", Version="...", Token="..."
        if (string.IsNullOrWhiteSpace(headerValue)) return null;
        if (!headerValue.StartsWith("MediaBrowser", StringComparison.OrdinalIgnoreCase)) return null;

        var idx = headerValue.IndexOf(' ');
        var paramPart = idx >= 0 ? headerValue[(idx + 1)..] : string.Empty;
        if (string.IsNullOrEmpty(paramPart)) return null;

        // Split by commas, then by =, strip quotes
        var segments = paramPart.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var seg in segments)
        {
            var kv = seg.Split('=', 2, StringSplitOptions.TrimEntries);
            if (kv.Length != 2) continue;
            var key = kv[0].Trim();
            var val = kv[1].Trim().Trim('"');
            if (key.Equals("Token", StringComparison.OrdinalIgnoreCase))
            {
                return val;
            }
        }

        // Handle single-key format: Token="..." only (no comma)
        if (paramPart.StartsWith("Token=", StringComparison.OrdinalIgnoreCase))
        {
            return paramPart.Substring(6).Trim().Trim('"');
        }

        return null;
    }
}
