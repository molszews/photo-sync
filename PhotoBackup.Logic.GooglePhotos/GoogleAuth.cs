using System;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.GData.Client;
using Google.GData.Photos;

namespace PhotoBackup.Logic.GooglePhotos
{
    public class GoogleAuth
    {
        private const string ApplicationName = "Uploader";
        private const string ClientId = "585272859136.apps.googleusercontent.com";
        private const string ClientSecret = "cAMagwFdOk7ISjcWeODFY1pA";

        private static readonly string[] scopes =
        {
            "https://www.googleapis.com/auth/userinfo#email",
            "https://mail.google.com/",
            "https://www.googleapis.com/auth/photos",
            "https://www.google.com/m8/feeds/",
            "https://picasaweb.google.com/c/",
            "https://www.googleapis.com/auth/plus.stream.write",
            "https://www.googleapis.com/auth/plus.circles.read",
            "https://www.googleapis.com/auth/plus.profiles.read",
            "https://www.googleapis.com/auth/plus.me",
            "https://www.googleapis.com/auth/plus.media.upload",
            "https://www.googleapis.com/auth/plus.media.readonly",
            "https://www.googleapis.com/auth/plus.settings",
            "http://gdata.youtube.com"
        };

        public static PicasaService GetInstance()
        {
            var credential =
                GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = ClientId,
                        ClientSecret = ClientSecret
                    },
                    scopes,
                    Environment.UserName,
                    CancellationToken.None,
                    new FileDataStore("Uploader.GooglePlus.Auth.Store")
                    ).Result;

            var wasTokenRefreshed = credential.RefreshTokenAsync(CancellationToken.None).Result;
            
            var requestFactory = new GDataRequestFactory(ApplicationName);
            requestFactory.CustomHeaders.Add($"Authorization: Bearer {credential.Token.AccessToken}");

            return new PicasaService(ApplicationName)
            {
                RequestFactory = requestFactory
            };
        }
    }
}