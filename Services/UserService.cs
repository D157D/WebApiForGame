using Crazy_Lobby.AppDataContext;
using Crazy_Lobby.Models;
using Microsoft.EntityFrameworkCore;

namespace Crazy_Lobby.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> AddFriendAsync(string currentPlayerId, string friendUsername)
        {
            var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Username == friendUsername || u.DisplayName == friendUsername);
            if (receiver == null) return (false, "Người dùng không tồn tại.");
            if (receiver.PlayerId == currentPlayerId) return (false, "Không thể kết bạn với chính mình.");

            var isAlreadyFriend = await _context.Friendships.AnyAsync(f => 
                (f.PlayerId1 == currentPlayerId && f.PlayerId2 == receiver.PlayerId) || 
                (f.PlayerId1 == receiver.PlayerId && f.PlayerId2 == currentPlayerId));
            if (isAlreadyFriend) return (false, "Hai người đã là bạn bè.");

            var existingRequest = await _context.FriendRequests.AnyAsync(f => 
                f.SenderId == currentPlayerId && f.ReceiverId == receiver.PlayerId && f.Status == "Pending");
            if (existingRequest) return (false, "Yêu cầu kết bạn đã được gửi trước đó.");

            var reverseRequest = await _context.FriendRequests.AnyAsync(f => 
                f.SenderId == receiver.PlayerId && f.ReceiverId == currentPlayerId && f.Status == "Pending");
            if (reverseRequest) return (false, "Người này đã gửi yêu cầu kết bạn cho bạn, vui lòng kiểm tra lời mời.");

            await _context.FriendRequests.AddAsync(new FriendRequest 
            { 
                SenderId = currentPlayerId, 
                ReceiverId = receiver.PlayerId, 
                Status = "Pending" 
            });
            await _context.SaveChangesAsync();

            return (true, $"Yêu cầu kết bạn đã được gửi đến {friendUsername}!");
        }

        public async Task<(bool Success, string Message)> AcceptFriendAsync(string currentPlayerId, string friendUsername)
        {
            var sender = await _context.Users.FirstOrDefaultAsync(u => u.Username == friendUsername || u.DisplayName == friendUsername);
            if (sender == null) return (false, "Người dùng không tồn tại.");

            var friendRequest = await _context.FriendRequests.FirstOrDefaultAsync(f => 
                f.SenderId == sender.PlayerId && f.ReceiverId == currentPlayerId && f.Status == "Pending");

            if (friendRequest == null) return (false, "Không tìm thấy yêu cầu kết bạn đang chờ.");

            friendRequest.Status = "Accepted";
            await _context.Friendships.AddAsync(new Friendship 
            { 
                PlayerId1 = sender.PlayerId, 
                PlayerId2 = currentPlayerId 
            });

            await _context.SaveChangesAsync();
            return (true, $"Bạn đã chấp nhận yêu cầu kết bạn từ {friendUsername}!");
        }

        public async Task<(bool Success, string Message)> DeclineFriendAsync(string currentPlayerId, string friendUsername)
        {
            var sender = await _context.Users.FirstOrDefaultAsync(u => u.Username == friendUsername || u.DisplayName == friendUsername);
            if (sender == null) return (false, "Người dùng không tồn tại.");

            var friendRequest = await _context.FriendRequests.FirstOrDefaultAsync(f => 
                f.SenderId == sender.PlayerId && f.ReceiverId == currentPlayerId && f.Status == "Pending");

            if (friendRequest == null) return (false, "Không tìm thấy yêu cầu kết bạn đang chờ.");

            _context.FriendRequests.Remove(friendRequest);
            await _context.SaveChangesAsync();

            return (true, $"Bạn đã từ chối yêu cầu kết bạn từ {friendUsername}!");
        }

        public async Task<(bool Success, string Message, int? InviteId)> InviteFriendAsync(string currentPlayerId, string friendUsername, string roomId)
        {
            var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Username == friendUsername || u.DisplayName == friendUsername);
            if (receiver == null) return (false, "Người dùng không tồn tại.", null);
            if (receiver.PlayerId == currentPlayerId) return (false, "Không thể mời chính mình.", null);

            var areFriends = await _context.Friendships.AnyAsync(f => 
                (f.PlayerId1 == currentPlayerId && f.PlayerId2 == receiver.PlayerId) || 
                (f.PlayerId1 == receiver.PlayerId && f.PlayerId2 == currentPlayerId));
            if (!areFriends) return (false, "Bạn chỉ có thể mời bạn bè chơi cùng.", null);

            var oldInvite = await _context.GameInvites.FirstOrDefaultAsync(i => 
                i.SenderId == currentPlayerId && i.ReceiverId == receiver.PlayerId && i.RoomId == roomId && i.Status == "Pending");
            if (oldInvite != null) _context.GameInvites.Remove(oldInvite);

            var invite = new GameInvite
            {
                SenderId = currentPlayerId,
                ReceiverId = receiver.PlayerId,
                RoomId = roomId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _context.GameInvites.AddAsync(invite);
            await _context.SaveChangesAsync();

            return (true, $"Bạn đã gửi lời mời chơi đến {friendUsername}!", invite.Id);
        }

        public async Task<IEnumerable<GameInviteResponse>> GetGameInvitesAsync(string currentPlayerId)
        {
            return await _context.GameInvites
                .AsNoTracking()
                .Where(i => i.ReceiverId == currentPlayerId && i.Status == "Pending")
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new GameInviteResponse
                {
                    InviteId = i.Id,
                    SenderUsername = _context.Users.Where(u => u.PlayerId == i.SenderId).Select(u => u.Username).FirstOrDefault(),
                    SenderDisplayName = _context.Users.Where(u => u.PlayerId == i.SenderId).Select(u => u.DisplayName).FirstOrDefault(),
                    RoomId = i.RoomId,
                    Status = i.Status,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message, string? Status, string? RoomId)> RespondToGameInviteAsync(string currentPlayerId, int inviteId, string status)
        {
            var invite = await _context.GameInvites.FirstOrDefaultAsync(i => i.Id == inviteId && i.ReceiverId == currentPlayerId);
            if (invite == null) return (false, "Không tìm thấy lời mời.", null, null);

            if (status != "Accepted" && status != "Declined")
                return (false, "Trạng thái không hợp lệ.", null, null);

            invite.Status = status;
            await _context.SaveChangesAsync();

            string message = status == "Accepted" ? "Bạn đã chấp nhận lời mời." : "Bạn đã từ chối lời mời.";
            return (true, message, invite.Status, invite.RoomId);
        }

        public async Task<(bool Success, string Message)> DeleteFriendAsync(string currentPlayerId, string friendUsername)
        {
            var friend = await _context.Users.FirstOrDefaultAsync(u => u.Username == friendUsername || u.DisplayName == friendUsername);
            if (friend == null) return (false, "Người dùng không tồn tại.");

            var friendship = await _context.Friendships.FirstOrDefaultAsync(f => 
                (f.PlayerId1 == currentPlayerId && f.PlayerId2 == friend.PlayerId) || 
                (f.PlayerId1 == friend.PlayerId && f.PlayerId2 == currentPlayerId));

            if (friendship == null) return (false, "Hai người không phải là bạn bè.");

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();

            return (true, $"Bạn đã xóa {friendUsername} khỏi danh sách bạn bè!");
        }

        public async Task<IEnumerable<FriendResponse>> GetFriendsAsync(string currentPlayerId)
        {
            var friendIds = await _context.Friendships
                .AsNoTracking()
                .Where(f => f.PlayerId1 == currentPlayerId || f.PlayerId2 == currentPlayerId)
                .Select(f => f.PlayerId1 == currentPlayerId ? f.PlayerId2 : f.PlayerId1)
                .ToListAsync();

            return await _context.Users
                .AsNoTracking()
                .Where(u => friendIds.Contains(u.PlayerId))
                .Select(u => new FriendResponse
                {
                    Username = u.Username!,
                    DisplayName = string.IsNullOrEmpty(u.DisplayName) ? u.Username! : u.DisplayName,
                    Status = "Online", // This could be improved with real-time status tracking
                    CharacterType = "default"
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<FriendRequestResponse>> GetFriendRequestsAsync(string currentPlayerId)
        {
            var requesterIds = await _context.FriendRequests
                .AsNoTracking()
                .Where(f => f.ReceiverId == currentPlayerId && f.Status == "Pending")
                .Select(f => f.SenderId)
                .ToListAsync();

            return await _context.Users
                .AsNoTracking()
                .Where(u => requesterIds.Contains(u.PlayerId))
                .Select(u => new FriendRequestResponse
                {
                    Username = u.Username!,
                    DisplayName = string.IsNullOrEmpty(u.DisplayName) ? u.Username! : u.DisplayName,
                    CharacterType = "default"
                })
                .ToListAsync();
        }

        public async Task<UserProfileResponse?> GetProfileAsync(string playerId)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.PlayerId == playerId)
                .Select(u => new UserProfileResponse
                {
                    Username = u.Username!,
                    DisplayName = string.IsNullOrEmpty(u.DisplayName) ? u.Username! : u.DisplayName
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string Message, string? DisplayName)> UpdateDisplayNameAsync(string playerId, string newDisplayName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PlayerId == playerId);
            if (user == null) return (false, "Người dùng không tồn tại.", null);

            var isDuplicate = await _context.Users.AnyAsync(u => u.DisplayName == newDisplayName && u.PlayerId != playerId);
            if (isDuplicate) return (false, "Tên hiển thị này đã được sử dụng bởi người chơi khác.", null);

            user.DisplayName = newDisplayName;
            await _context.SaveChangesAsync();

            return (true, "Đổi tên hiển thị thành công!", user.DisplayName);
        }

        public async Task<IEnumerable<FriendResponse>> SearchUsersAsync(string currentPlayerId, string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<FriendResponse>();

            return await _context.Users
                .AsNoTracking()
                .Where(u => u.PlayerId != currentPlayerId && (u.Username!.Contains(query) || (u.DisplayName != null && u.DisplayName.Contains(query))))
                .Take(20)
                .Select(u => new FriendResponse
                {
                    Username = u.Username!,
                    DisplayName = string.IsNullOrEmpty(u.DisplayName) ? u.Username! : u.DisplayName,
                    Status = "Online",
                    CharacterType = "default"
                })
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(string playerId)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.PlayerId == playerId);
        }

        public async Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> playerIds)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => playerIds.Contains(u.PlayerId))
                .ToListAsync();
        }

        public async Task<User?> GetUserByUsernameOrDisplayNameAsync(string identifier)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == identifier || u.DisplayName == identifier);
        }
    }
}
