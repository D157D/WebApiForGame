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
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<GameInvite> GameInvites { get; set; }
        
    }
}