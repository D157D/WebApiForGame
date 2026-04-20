using Crazy_Lobby.Models;

namespace Crazy_Lobby.Services
{
    public interface IUserService
    {
        Task<(bool Success, string Message)> AddFriendAsync(string currentPlayerId, string friendUsername);
        Task<(bool Success, string Message)> AcceptFriendAsync(string currentPlayerId, string friendUsername);
        Task<(bool Success, string Message)> DeclineFriendAsync(string currentPlayerId, string friendUsername);
        Task<(bool Success, string Message, int? InviteId)> InviteFriendAsync(string currentPlayerId, string friendUsername, string roomId);
        Task<IEnumerable<GameInviteResponse>> GetGameInvitesAsync(string currentPlayerId);
        Task<(bool Success, string Message, string? Status, string? RoomId)> RespondToGameInviteAsync(string currentPlayerId, int inviteId, string status);
        Task<(bool Success, string Message)> DeleteFriendAsync(string currentPlayerId, string friendUsername);
        Task<IEnumerable<FriendResponse>> GetFriendsAsync(string currentPlayerId);
        Task<IEnumerable<FriendRequestResponse>> GetFriendRequestsAsync(string currentPlayerId);
        Task<UserProfileResponse?> GetProfileAsync(string playerId);
        Task<(bool Success, string Message, string? DisplayName)> UpdateDisplayNameAsync(string playerId, string newDisplayName);
        Task<IEnumerable<FriendResponse>> SearchUsersAsync(string currentPlayerId, string query);
        Task<User?> GetUserByIdAsync(string playerId);
        Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> playerIds);
        Task<User?> GetUserByUsernameOrDisplayNameAsync(string identifier);
    }
}
