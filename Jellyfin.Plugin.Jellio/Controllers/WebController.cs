using System.Net.Mime;
using System.Reflection;
using Jellyfin.Plugin.Jellio.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.Jellio.Controllers;

[ApiController]
[Route("jellio")]
public class WebController(
    IUserManager userManager,
    IUserViewManager userViewManager,
    IDtoService dtoService,
    IServerApplicationHost serverApplicationHost
) : ControllerBase
{
    private readonly Assembly _executingAssembly = Assembly.GetExecutingAssembly();

    [AllowAnonymous]
    [HttpGet]
    [HttpGet("configure")]
    [HttpGet("{config?}/configure")]
    public IActionResult GetIndex(string? config = null)
    {
        const string ResourceName = "Jellyfin.Plugin.Jellio.Web.config.html";

        var resourceStream = _executingAssembly.GetManifestResourceStream(ResourceName);

        if (resourceStream == null)
        {
            return NotFound($"Resource {ResourceName} not found.");
        }

        return new FileStreamResult(resourceStream, "text/html");
    }

    [Authorize]
    [HttpGet("server-info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(MediaTypeNames.Application.Json)]
    public IActionResult GetServerInfo()
    {
        var user = RequestHelpers.GetCurrentUser(User, userManager);
        if (user == null)
        {
            return Unauthorized();
        }

        var friendlyName = serverApplicationHost.FriendlyName;
        var libraries = LibraryHelper.GetUserLibraries(user, userViewManager, dtoService);

        return Ok(new { name = friendlyName, libraries });
    }
}
