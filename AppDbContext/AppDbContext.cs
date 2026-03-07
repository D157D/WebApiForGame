using Microsoft.EntityFrameworkCore;
using Sever.Models;

namespace Crazy_Lobby.AppDbContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<MatchResult> MatchResults { get; set; }
    }
}