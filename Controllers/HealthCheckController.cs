using Microsoft.AspNetCore.Mvc;

namespace dotnet_bookish_starter.Controllers;

[ApiController]
[Route("healthcheck")]
public class HealthCheckController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "ok" });
    }
}
