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
using SharedLibrary;
using TinyMessenger;
using GoogleAlbum=Google.Picasa.Album;

namespace PhotoBackup.Logic.GooglePhotos
{
    public class GooglePhotoUploader : IPhotoUploader
    {
        private readonly PicasaService _picasaService;
        private readonly IGoogleAlbumProvider _googleAlbumProvider;
        private readonly GoogleLimitations _googleLimitationService;
        private readonly ITinyMessengerHub _messageHub;

        public GooglePhotoUploader(PicasaService picasaService, 
            IGoogleAlbumProvider googleAlbumProvider,
            GoogleLimitations googleLimitationService,
            ITinyMessengerHub messageHub)
        {
            _picasaService = picasaService;
            _googleAlbumProvider = googleAlbumProvider;
            _googleLimitationService = googleLimitationService;
            _messageHub = messageHub;
        }

        public void Upload(IEnumerable<DiskPhoto> photos)
        {
            var filteredPhotos = FilterItems(photos).ToList();
            var photosByAlbum = filteredPhotos.GroupBy(p => p.Album, p => p);
            foreach (var albumPhotos in photosByAlbum)
            {
                var album = albumPhotos.Key;
                GoogleAlbum googleAlbum;

                try
                {
                    googleAlbum = GetOrCreateAlbum(album);
                }
                catch (Exception e)
                {
                    _messageHub.Publish(new AlbumUploadFailed(album, e));
                    continue;
                }

                _messageHub.Publish(new AlbumUploadStart(album));
                UploadPhotosFromAlbum(googleAlbum, albumPhotos);
                _messageHub.Publish(new AlbumUploadEnd(album));
            }
        }

        private IEnumerable<DiskPhoto> FilterItems(IEnumerable<DiskPhoto> photos)
        {
            foreach (var diskPhoto in photos)
            {
                if (_googleLimitationService.IsValid(diskPhoto))
                {
                    yield return diskPhoto;
                }
                else
                {
                    _messageHub.Publish(new PhotoUploadSkipped(diskPhoto));
                }
            }
        }

        private void UploadPhotosFromAlbum(GoogleAlbum album, IEnumerable<DiskPhoto> albumPhotos)
        {
            foreach(var photo in albumPhotos)
            {
                _messageHub.Publish(new PhotoUploadStart(photo));

                GooglePhoto ret = null;
                Utils.Retry(() => ret = UploadPhoto(album, photo),
                    ex => _messageHub.Publish(new PhotoUploadFailed(photo, ex)),
                    30);
                if(ret == null) continue;

                _messageHub.Publish(new PhotoUploadEnd(photo));
            }
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

        private GooglePhoto UploadPhoto(Google.Picasa.Album album, DiskPhoto photo)
        {
            using (var fs = new FileStream(photo.FilePath, FileMode.Open))
            {
                var newUri = new Uri(PicasaQuery.CreatePicasaUri("default", album.Id));
                var newEntry = (PicasaEntry)_picasaService.Insert(newUri, fs, "Image/jpeg", photo.Title);
                var newPhoto = new Photo {AtomEntry = newEntry};
                return photo.ToGooglePhoto();
            }
        }

    }
}