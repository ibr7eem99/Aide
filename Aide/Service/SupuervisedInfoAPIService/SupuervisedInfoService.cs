using Aide.Data;
using Aide.Extensions;
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
        public int GetAccessToken(Login login, HttpContext context)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("grant_type", login.grand_type);
            dict.Add("client_id", login.client_id);
            dict.Add("scope", login.scope);
            dict.Add("username", login.Username);
            dict.Add("password", login.Password);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://id.asu.edu.jo");
                var responce = client.PostAsync("connect/token", new FormUrlEncodedContent(dict));
                responce.Wait();
                var result = responce.Result;
                if (result.IsSuccessStatusCode)
                {
                    HttpContent content = result.Content;
                    string jsoncontent = content.ReadAsStringAsync().Result;
                    CookiSpace.AddToken(context, jsoncontent);
                    CookiSpace.AddUser(context);
                    return (int)result.StatusCode;
                }
                return (int)result.StatusCode;
            }
        }

        public IEnumerable<Supuervised> GetSupuervisedInfo(HttpContext context, StudentPlanInfo model, string passCode)
        {
            IEnumerable<Supuervised> Supuervised = null;
            Token token = CookiSpace.GetToken(context);
            string user = CookiSpace.GetUser(context);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.asu.edu.jo/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.token_type, token.access_token);
                int semester = (int)model.Semester;
                /*ProfessorInfo professor = new ProfessorInfo { Username = "w_manaseer", passCode = passCode };*/
                ProfessorInfo professor = new ProfessorInfo { Username = "m_aloudat", passCode = passCode };
                /*ProfessorInfo professor = new ProfessorInfo { Username = "a_abusamaha", passCode = passCode };*/
                /*ProfessorInfo professor = new ProfessorInfo { Username = "m_albashayreh", passCode = passCode };*/
                /*ProfessorInfo professor = new ProfessorInfo { Username = "y_alqasrawi", passCode = passCode };*/
                var responce = client.PostAsJsonAsync($"api/Courses/Supervisored?year={model.Year}&semester={semester}", professor);
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
