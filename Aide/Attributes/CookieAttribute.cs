using Aide.Data;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Aide.Attribute
{
    public static class CookieAttribute
    {
        public static void AddToken(HttpContext httpContext, string jsoncontent)
        {
            httpContext.Session.Set("token", System.Text.Encoding.ASCII.GetBytes(jsoncontent));
        }

        public static void AddUser(HttpContext httpContext)
        {
            httpContext.Session.Set("user", System.Text.Encoding.ASCII.GetBytes("m_albashayreh"));
            /*httpContext.Session.Set("user", System.Text.Encoding.ASCII.GetBytes("m_aloudat"));*/
            /*httpContext.Session.Set("user", System.Text.Encoding.ASCII.GetBytes("a_abusamaha"));*/
            /*httpContext.Session.Set("user", System.Text.Encoding.ASCII.GetBytes("y_alqasrawi"));*/
            /*httpContext.Session.Set("user", System.Text.Encoding.ASCII.GetBytes("w_manaseer"));*/
            /*httpContext.Session.Set("user", System.Text.Encoding.ASCII.GetBytes("b_kasasbeh"));*/
        }

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
