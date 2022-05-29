using Aide.Data;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Aide.Extensions
{
    public static class CookiSpace
    {
        public static Token GetToken(HttpContext httpContext)
        {
            byte[] tokenbyts = null;
            string tokenvalue = null;
            if (httpContext.Session.TryGetValue("token", out tokenbyts))
            {
                tokenvalue = System.Text.Encoding.ASCII.GetString(tokenbyts);
                return JsonSerializer.Deserialize<Token>(tokenvalue);
            }
            return null;
        }

        public static string GetUser(HttpContext httpContext)
        {
            byte[] tokenbyts = null;
            string tokenvalue = null;
            if (httpContext.Session.TryGetValue("user", out tokenbyts))
            {
                tokenvalue = System.Text.Encoding.ASCII.GetString(tokenbyts);
            }
            return tokenvalue;
        }

        public static void ClearAllCookis(HttpRequest request, HttpResponse response)
        {
            foreach (var key in request.Cookies.Keys)
            {
                response.Cookies.Delete(key);
            }
        }
    }
}
