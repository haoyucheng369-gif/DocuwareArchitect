namespace Platform.Identity;

public interface IUserIdentityService
{
    UserIdentity Login(string username, string password);
    UserIdentity GetCurrentUser();
}
