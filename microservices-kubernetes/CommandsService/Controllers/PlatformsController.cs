using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers;

[ApiController]
[Route("api/c/[controller]")]
public class PlatformsController : ControllerBase
{
    public PlatformsController()
    {

    }


    [HttpGet]
    public async Task<ActionResult> TestInboundConnection()
    {
        Console.WriteLine("Hello from TestInboundConnection");
        return Ok("Hello from TestInboundConnection");
    }

}

