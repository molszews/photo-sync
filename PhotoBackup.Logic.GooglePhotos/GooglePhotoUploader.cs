using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Google.GData.Photos;
using Google.Picasa;
using PhotoBackup.Logic.Messages;
using TinyMessenger;

namespace PhotoBackup.Logic.GooglePhotos
{
    public class GooglePhotoUploader : IPhotoUploader
    {
        private readonly PicasaService _picasaService;
        private readonly IGoogleAlbumProvider _googleAlbumProvider;
        private readonly ITinyMessengerHub _messageHub;
        private readonly GoogleLimitations _googleLimitations;

        public GooglePhotoUploader(PicasaService picasaService, 
            IGoogleAlbumProvider googleAlbumProvider, 
            ITinyMessengerHub messageHub,
            GoogleLimitations googleLimitations)
        {
            _picasaService = picasaService;
            _googleAlbumProvider = googleAlbumProvider;
            _messageHub = messageHub;
            _googleLimitations = googleLimitations;
        }

        public void UploadPhotos(IEnumerable<DiskPhoto> photos)
        {
            var filteredPhotos = FilterItems(photos);

            var photosByAlbum = filteredPhotos.GroupBy(p => p.Album, p => p);
            foreach (var albumPhotos in photosByAlbum)
            {
                var album = albumPhotos.Key;

                UploadPhotosFromAlbum(album, albumPhotos.ToList());
            }
        }

        private IEnumerable<DiskPhoto> FilterItems(IEnumerable<DiskPhoto> photos)
        {
            foreach (var diskPhoto in photos)
            {
                var fileInfo = new FileInfo(diskPhoto.Url);
                if (_googleLimitations.IsValid(fileInfo))
                {
                    yield return diskPhoto;
                }
                else
                {
                    _messageHub.Publish(new PhotoUploadSkipped(diskPhoto));
                }
            }
        }

        private void UploadPhotosFromAlbum(Album album, IList<DiskPhoto> albumPhotos)
        {
            _messageHub.Publish(new AlbumUploadStart(album));
            Google.Picasa.Album googleAlbum = null;

            try
            {
                googleAlbum = GetOrCreateAlbum(album);
            }
            catch (Exception e)
            {
                _messageHub.Publish(new AlbumUploadFailed(album, e));
                return;
            }

            var uploadedPhotos = new ConcurrentBag<GooglePhoto>();

            var maxDegreeOfParallelism = 4;
            Parallel.ForEach(albumPhotos, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, photo =>
            {
                _messageHub.Publish(new PhotoUploadStart(photo));
                var flickrPhoto = UploadPhotoWithRetry(googleAlbum, photo);
                uploadedPhotos.Add(flickrPhoto);

                _messageHub.Publish(new PhotoUploadEnd(photo));
            });

            _messageHub.Publish(new AlbumUploadEnd(album));
        }

        private Google.Picasa.Album GetOrCreateAlbum(Album album)
        {
            var googleAlbum =
                _googleAlbumProvider.GetAlbums()
                    .SingleOrDefault(a => a.Title.Equals(album.Title.LimitLength(100), StringComparison.InvariantCultureIgnoreCase));

            return googleAlbum ?? CreateAlbum(album);
        }

        private Google.Picasa.Album CreateAlbum(Album album)
        {
            var newEntry = new AlbumEntry {Title = {Text = album.Title.LimitLength(100) } };
            var newAlbum = new Google.Picasa.Album
            {
                AtomEntry = newEntry
            };
            newAlbum.Access = "private";

            var feedUri = new Uri(PicasaQuery.CreatePicasaUri("default"));

            var picasaEntry = _picasaService.Insert(feedUri, newEntry);
            return new Google.Picasa.Album {AtomEntry = picasaEntry};
        }
        
        private GooglePhoto UploadPhotoWithRetry(Google.Picasa.Album album, DiskPhoto photo, int retriesLeft = 3)
        {
            try
            {
                return UploadPhoto(album, photo);
            }
            catch (Exception e)
            {
                _messageHub.Publish(new PhotoUploadFailed(photo, e, retriesLeft));

                if (retriesLeft > 0)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    return UploadPhotoWithRetry(album, photo, --retriesLeft);
                }

                return new GooglePhoto() { Id = "-1" };
                //                throw;
            }
        }

        private GooglePhoto UploadPhoto(Google.Picasa.Album album, DiskPhoto photo)
        {
            using (var fs = new FileStream(photo.Url, FileMode.Open))
            {
//                Thread.Sleep(5000);
                var newUri = new Uri(PicasaQuery.CreatePicasaUri("default", album.Id));
                var newEntry = (PicasaEntry)_picasaService.Insert(newUri, fs, "Image/jpeg", photo.Title);
                var newPhoto = new Photo {AtomEntry = newEntry};

                var googlePhoto = photo.ToGooglePhoto();
                googlePhoto.Id = newPhoto.Id;
                return googlePhoto;
            }
        }
    }
}