using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Crazy_Lobby.Services;
using Crazy_Lobby.AppDataContext;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly AppDbContext _dbContext;

    public MatchController(IRoomService roomService, AppDbContext dbContext)
    {
        _roomService = roomService;
        _dbContext = dbContext;
    }
}