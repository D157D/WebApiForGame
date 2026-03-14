using Crazy_Lobby.Models;
using Microsoft.EntityFrameworkCore;

namespace Crazy_Lobby.AppDataContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}