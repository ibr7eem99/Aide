using Aide.Attribute;
using Aide.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Aide.Service.SupuervisedInfoAPIService
{
    public class SupuervisedInfoService : ISupuervisedInfoService
    {
        private Dictionary<string, string> UrlContentValuePairs { get; set; }

        public int GetAccessToken(Login login, HttpContext context)
        {
            UrlContentValuePairs = new Dictionary<string, string>();
            UrlContentValuePairs.Add("grant_type", login.grand_type);
            UrlContentValuePairs.Add("client_id", login.client_id);
            UrlContentValuePairs.Add("scope", login.scope);
            UrlContentValuePairs.Add("username", login.Username);
            UrlContentValuePairs.Add("password", login.Password);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://id.asu.edu.jo");
                var responce = client.PostAsync("connect/token", new FormUrlEncodedContent(UrlContentValuePairs));
                responce.Wait();
                var result = responce.Result;
                if (result.IsSuccessStatusCode)
                {
                    HttpContent content = result.Content;
                    string jsoncontent = content.ReadAsStringAsync().Result;
                    CookieAttribute.AddToken(context, jsoncontent);
                    CookieAttribute.AddUser(context);
                    return (int)result.StatusCode;
                }
                return (int)result.StatusCode;
            }
        }

        public IEnumerable<Supuervised> GetSupuervisedInfo(HttpContext context, StudentPlanInfo model, ProfessorInfo professorInfo)
        {
            IEnumerable<Supuervised> Supuervised = null;
            Token token = CookieAttribute.GetToken(context);
            string user = CookieAttribute.GetUser(context);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.asu.edu.jo/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.token_type, token.access_token);
                int semester = (int)model.Semester;
                var responce = client.PostAsJsonAsync($"api/Courses/Supervisored?year={model.Year}&semester={semester}", professorInfo);
                responce.Wait();
                var result = responce.Result;
                if (result.IsSuccessStatusCode)
                {
                    HttpContent httpContent = result.Content;
                    string jsoncontent = httpContent.ReadAsStringAsync().Result;
                    Supuervised = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Supuervised>>(jsoncontent);
                }
                else
                {
                    Supuervised = Enumerable.Empty<Supuervised>();
                }
            }
            return Supuervised;
        }
    }
}
