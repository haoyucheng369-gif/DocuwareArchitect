namespace Platform.Identity;

public record UserIdentity(string Username, string Token);

public class SimpleIdentityService : IUserIdentityService
{
    private UserIdentity? _currentUser;

    public UserIdentity Login(string username, string password)
    {
        // 简单模拟身份验证，真实场景应调用 Identity Provider 或 OAuth/OpenID Connect。
        _currentUser = new UserIdentity(username, $"token-{username}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
        return _currentUser;
    }

    public UserIdentity GetCurrentUser()
    {
        return _currentUser ?? throw new InvalidOperationException("User has not logged in yet.");
    }
}
