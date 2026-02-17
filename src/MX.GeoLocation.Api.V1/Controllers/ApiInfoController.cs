using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MX.GeoLocation.LookupWebApi.Controllers;

[ApiController]
[AllowAnonymous]
[ApiVersionNeutral]
public class ApiInfoController : ControllerBase
{
    [HttpGet("/api/info")]
    public IActionResult GetInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unknown";
        var assemblyVersion = assembly.GetName().Version?.ToString() ?? "unknown";

        return Ok(new
        {
            version = informationalVersion,
            assemblyVersion
        });
    }
}
