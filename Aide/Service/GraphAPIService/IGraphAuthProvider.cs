using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace Aide.Service.GraphAPIService
{
    public interface IGraphAuthProvider
    {
        string Authority { get; }

        Task<string> GetUserAccessTokenAsync(string userId);

        Task<AuthenticationResult> GetUserAccessTokenByAuthorizationCode(string authorizationCode);
    }
}
