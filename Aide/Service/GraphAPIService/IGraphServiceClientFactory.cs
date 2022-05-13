using Microsoft.Graph;
using System.Security.Claims;

namespace Aide.Service.GraphAPIService
{
    public interface IGraphServiceClientFactory
    {
        GraphServiceClient GetAuthenticatedGraphClient(ClaimsIdentity userIdentity);
    }
}
