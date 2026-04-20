using Microsoft.AspNetCore.Mvc;

namespace Crazy_Lobby.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected string? CurrentPlayerId => User.FindFirst("PlayerId")?.Value;
    }
}
