using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlickrNet;

namespace FlickrUploader
{
    /// <summary>
    /// http://flickr.com/services/auth/?api_key=[api_key]&perms=[perms]&api_sig=[api_sig]
    /// 
    /// http://www.flickr.com/services/auth/?api_key=5ef321f7d738bb20cf45099f8da2116d&perms=delete&api_sig=0160dff41541e0aec366910807add2e1
    /// 
    /// 0b13557b80020b9dapi_key5ef321f7d738bb20cf45099f8da2116dpermsdelete
    /// 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var flickr = new FlickrNet.Flickr("5ef321f7d738bb20cf45099f8da2116d", "0b13557b80020b9d");
           
            var z = flickr.OAuthGetRequestToken("http://studioolszak.pl");
            var url = flickr.OAuthCalculateAuthorizationUrl(z.Token, AuthLevel.Delete);
            var accessToken = flickr.OAuthGetAccessToken(z, "de53ecb56499e460");

            var login = flickr.TestLogin();

            flickr.GalleriesCreate("TestGallery01", "desc");

            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
