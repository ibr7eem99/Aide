using Aide.Data;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Aide.Service.SupuervisedInfoAPIService
{
    public interface ISupuervisedInfoService
    {
        public int GetAccessToken(Login login, HttpContext context);
        public IEnumerable<Supuervised> GetSupuervisedInfo(HttpContext context, StudentPlanInfo model, string passCode);
    }
}
