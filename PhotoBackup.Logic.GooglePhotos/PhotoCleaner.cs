using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.GData.Photos;
using Google.Picasa;
using PhotoBackup.Logic.Messages;
using TinyMessenger;

namespace PhotoBackup.Logic.GooglePhotos
{
    public class PhotoCleaner : IPhotoCleaner
    {
        private readonly ITinyMessengerHub _messageHub;
        private readonly PicasaService _service;
        private readonly GooglePhotoProvider _googlePhotoProvider;

        public PhotoCleaner(ITinyMessengerHub messageHub, PicasaService service, GooglePhotoProvider googlePhotoProvider)
        {
            _messageHub = messageHub;
            _service = service;
            _googlePhotoProvider = googlePhotoProvider;
        }

        public void WipeAllPhotos()
        {
            var query = new AlbumQuery(PicasaQuery.CreatePicasaUri("default"));
            var feed = _service.Query(query);
            var albums = feed.Entries.Select(e => new Google.Picasa.Album {AtomEntry = e});

            foreach (var album in albums)
            {
                Console.WriteLine($"deleting {album.Title}");
                var query2 = new PhotoQuery(PicasaQuery.CreatePicasaUri("default", album.Id));
                var feed2 = _service.Query(query2);
                var photos = feed2.Entries.Select(e => new Photo {AtomEntry = e});

                Parallel.ForEach(photos, new ParallelOptions
                {
                    MaxDegreeOfParallelism = 8}, photo =>
//                foreach (var photo in photos)
                {
                    if (photo.ReadOnly) return;
                    Console.WriteLine($"deleting {photo.Title}");
                    try
                    {
                        photo.AtomEntry.Delete();
                        Console.WriteLine($"deleted {photo.Title}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"unable to delete {photo.Title}: {e.Message}");
                    }
                });
                try
                {
                    if (!album.ReadOnly)
                        album.AtomEntry.Delete();
                    Console.WriteLine($"deleted {album.Title}");

                }
                catch (Exception e)
                {
                    Console.WriteLine($"unable to delete {album.Title}: {e.Message}");
                }
            }
        }
    }
}