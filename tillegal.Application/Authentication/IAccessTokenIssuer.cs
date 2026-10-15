namespace Tillegal.Application.Authentication;

public interface IAccessTokenIssuer
{
    AccessTokenResult Issue(UserAccount user);
}
