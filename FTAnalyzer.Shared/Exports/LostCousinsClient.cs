using FTAnalyzer.Utilities;
using FTAnalyzer.Windows;
using System.Net;

namespace FTAnalyzer.Exports
{
    public class LostCousinsClient
    {
        public HttpClient Client { get; private set; }
        public CookieContainer Cookies { get; private set; }

        public bool LoggedIn { get; private set; }

        public LostCousinsClient()
        {
            SetupHttpClient();
        }

        void SetupHttpClient()
        {
            Cookies = new();
            HttpClientHandler handler = new()
            {
                CookieContainer = Cookies
            };
            Client = new(handler);
        }
        public async Task<bool> LostCousinsLoginAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    return false;
                if (password.Length > 15)
                    password = password[..15];
                Random random = new();
                Uri uri = new(@"https://www.lostcousins.com/pages/login/");
                Dictionary<string, string> parameters = new()
                {
                    { "stage", "submit" },
                    { "email", email },
                    { "password", password },
                    { "x", random.Next(1,99).ToString() },
                    { "y", random.Next(1,9).ToString() }
                };
                LoggedIn = false;
                HttpRequestMessage req = new(HttpMethod.Post, uri)
                {
                    Content = new FormUrlEncodedContent(parameters)
                };
                req.Content.Headers.Clear();
                req.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                HttpResponseMessage response = await Client.SendAsync(req);
                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    IEnumerable<Cookie> cookies = Cookies.GetCookies(uri).Cast<Cookie>();
                    LoggedIn = cookies.Count() == 2 && (cookies.Any(x => x.Name == "lostcousins_user_login" || x.Name == "lostcousins_user_login"));
                }
            }
            catch (Exception e)
            {
                UIHelpers.ShowMessage($"Problem accessing Lost Cousins Website. Check you are connected to internet. Error message is: {e.Message}");
                return false;
            }
            return LoggedIn;
        }

        public async Task<string> GetAncestors()
        {
            if (LoggedIn)
            {
                Uri uri = new("https://www.lostcousins.com/pages/members/ancestors/");
                HttpResponseMessage response = await Client.GetAsync(uri);
                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            return string.Empty;
        }

        public void EmptyCookieJar()
        {
            // TODO: remove cookies
        }
    }
}