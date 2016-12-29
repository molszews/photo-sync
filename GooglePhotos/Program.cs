using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.GData.Client;
using Google.GData.Photos;
using Google.Picasa;
using GooglePhotos.PhotoApi;

namespace GooglePhotos
{
    internal class Program
    {
        private static PicasaService AuthenticatePicasaServiceNonAsync(string accessToken)
        {
            var requestFactory = new GDataRequestFactory("My App User Agent");
            requestFactory.CustomHeaders.Add(string.Format("Authorization: Bearer {0}", accessToken));
            var ps_Picasa = new PicasaService("Uploader");
            ps_Picasa.RequestFactory = requestFactory;

            return ps_Picasa;
        }

        private static void Main(string[] args)
        {
            var credential =
                GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
                {
                    ClientId = GooglePhotoBackup.ClientId,
                    ClientSecret = GooglePhotoBackup.ClientSecret
                },
                    new[]
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
                    },
                    Environment.UserName,
                    CancellationToken.None,
                    new FileDataStore("Uploader.GooglePlus.Auth.Store")
                    ).Result;
            var x = credential.RefreshTokenAsync(CancellationToken.None).Result;


            var service = AuthenticatePicasaServiceNonAsync(credential.Token.AccessToken);

//            InsertAlbum(service);


            var albums = ListAlbums(service);
            var newAlbum = albums.Single(a => a.Title == "New album");

            var newURI = new Uri(PicasaQuery.CreatePicasaUri("default", newAlbum.Id));

            var newFile = new FileInfo(@"D:\2016\test\testalbum\IMG_5978.JPG");
            var neFStream = newFile.OpenRead();
            var newEntry = (PicasaEntry) service.Insert(newURI, neFStream, "Image/jpeg", "IMG_5978.JPG");
            neFStream.Close();

            Console.ReadKey();
        }

        private static void InsertAlbum(PicasaService service)
        {
            var newEntry = new AlbumEntry();
            newEntry.Title.Text = "New album";
            newEntry.Summary.Text = "This is an album";
            var ac = new AlbumAccessor(newEntry);
            //set to "private" for a private album
            ac.Access = "public";

            var feedUri = new Uri(PicasaQuery.CreatePicasaUri("default"));

            var createdEntry = service.Insert(feedUri, newEntry);
        }

        private static IEnumerable<Album> ListAlbums(PicasaService service)
        {
            var query = new AlbumQuery(PicasaQuery.CreatePicasaUri("default"));
            var feed = service.Query(query);
            return feed.Entries.Select(e => new Album {AtomEntry = e});
        }
    }
}