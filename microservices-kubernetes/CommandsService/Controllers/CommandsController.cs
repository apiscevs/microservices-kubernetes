using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers;

[ApiController]
[Route("api/c/[controller]")]
public class CommandsController : ControllerBase
{
    public CommandsController()
    {

    }
}

