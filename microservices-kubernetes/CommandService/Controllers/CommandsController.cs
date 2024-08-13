using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers;

[ApiController]
[Route("api/c/[controller]")]
public class CommandsController : ControllerBase
{
    public CommandsController()
    {

    }
}

