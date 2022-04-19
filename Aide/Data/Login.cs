namespace Aide.Data
{
    public class Login
    {
        public string grand_type { get; set; } = "password";
        public string client_id { get; set; } = "AOMAPI";
        public string scope { get; set; } = "offline_access openid profile ASUOnlineMobileApi";
        public string username { get; set; }
        public string password { get; set; }
    }
}
