using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace Aide.Extensions
{
    public class SessionExpiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!Enumerable.Any(filterContext.HttpContext.Session.Keys))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "action", "SignedOut" },
                    { "controller", "Accounts" }
                });
                base.OnActionExecuting(filterContext);
            }
        }
    }
}
