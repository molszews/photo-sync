using FlickrNet;

namespace PhotoBackup.Logic.Flickr
{
    public class FlickrAuth
    {
        public const string ApiKey = "5ef321f7d738bb20cf45099f8da2116d";
        public const string SharedSecret = "0b13557b80020b9d";

        public static FlickrNet.Flickr GetInstance()
        {
            return new FlickrNet.Flickr(ApiKey, SharedSecret);
        }

        public static FlickrNet.Flickr GetAuthInstance()
        {
            var f = new FlickrNet.Flickr(ApiKey, SharedSecret);
            f.OAuthAccessToken = "72157664809229141-41c9afa983750497";//OAuthToken.Token;
            f.OAuthAccessTokenSecret = "715ace90ff38b641";//OAuthToken.TokenSecret;
            return f;
        }

        public static OAuthAccessToken OAuthToken
        {
            get
            {
                return Properties.Settings.Default.OAuthToken;
            }
            set
            {
                Properties.Settings.Default.OAuthToken = value;
                Properties.Settings.Default.Save();
            }
        }

    }
}
