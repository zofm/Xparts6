using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Smartstore.FatturazioneElettronica.Providers.Aruba.Common;
using Smartstore.FatturazioneElettronica.Providers.Aruba.Models;

namespace Smartstore.FatturazioneElettronica.Providers.Aruba
{
    public class ArubaAuthentication
    {
        private readonly string _baseAuthUrl;
        private readonly string _username;
        private readonly string _password;

        public ArubaAuthentication(string baseAuthUrl, string username, string password)
        {
            CurrentAccessToken = null;
            ForceRefreshToken = false;
            _baseAuthUrl = baseAuthUrl;
            _username = username;
            _password = password;
        }

        private string CurrentAccessToken { get; set; }
        private DateTime AccessTokenExpireDate { get; set; }
        private string CurrentRefreshToken { get; set; }
        public bool ForceRefreshToken { get; set; }

        public string AccessToken
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentAccessToken) || AccessTokenExpireDate == default)
                    SignIn();
                else if (DateTime.Now >= AccessTokenExpireDate || ForceRefreshToken)
                    RefreshToken();
                return CurrentAccessToken;
            }
        }

        private void SignIn()
        {
            var poststring = string.Format("grant_type=password&username={0}&password={1}", _username, _password);
            var httpRequest = (HttpWebRequest)WebRequest.Create(_baseAuthUrl + Constants.SigninPath);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            var bytedata = Encoding.UTF8.GetBytes(poststring);
            httpRequest.ContentLength = bytedata.Length;

            using (var requestStream = httpRequest.GetRequestStream())
                requestStream.Write(bytedata, 0, bytedata.Length);

            var httpWebResponse = (HttpWebResponse)httpRequest.GetResponse();

            if (httpWebResponse.StatusCode != HttpStatusCode.OK)
            {
                if (httpWebResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    using (var reader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var objText = reader.ReadToEnd();
                        var arubaResponse = JsonConvert.DeserializeObject<UnauthorizedLoginResponse>(objText);
                        throw new Exception(string.Format("Impossibile effettuare il login ({0} {1})", arubaResponse.ErrorCode, arubaResponse.ErrorDescription));
                    }
                }
                else
                {
                    throw new Exception("Impossibile effettuare il login");
                }
            }

            using (var reader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                var objText = reader.ReadToEnd();
                var arubaResponse = JsonConvert.DeserializeObject<LoginResponse>(objText);
                AccessTokenExpireDate = DateTime.Now.AddMinutes(arubaResponse.ExpiresIn - 10);
                CurrentAccessToken = arubaResponse.AccessToken;
                CurrentRefreshToken = arubaResponse.RefreshToken;
            }
        }

        private void RefreshToken()
        {
            if (string.IsNullOrEmpty(CurrentRefreshToken))
            {
                SignIn();
            }
            else
            {
                var poststring = string.Format("grant_type=refresh_token&refresh_token={0}", CurrentRefreshToken);
                var httpRequest = (HttpWebRequest)WebRequest.Create(_baseAuthUrl + Constants.SigninPath);
                httpRequest.Method = "POST";
                httpRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                var bytedata = Encoding.UTF8.GetBytes(poststring);
                httpRequest.ContentLength = bytedata.Length;

                using (var requestStream = httpRequest.GetRequestStream())
                    requestStream.Write(bytedata, 0, bytedata.Length);

                var httpWebResponse = (HttpWebResponse)httpRequest.GetResponse();

                if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                {
                    SignIn();
                }
                else
                {
                    using (var reader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var objText = reader.ReadToEnd();
                        var arubaResponse = JsonConvert.DeserializeObject<LoginResponse>(objText);
                        AccessTokenExpireDate = DateTime.Now.AddMinutes(arubaResponse.ExpiresIn - 10);
                        CurrentAccessToken = arubaResponse.AccessToken;
                        CurrentRefreshToken = arubaResponse.RefreshToken;
                    }
                }
            }
        }
    }
}
