using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlickrNet;
using PhotoBackup.Logic.Messages;
using SharedLibrary;
using TinyMessenger;

namespace PhotoBackup.Logic.Flickr
{
    public class FlickrPhotoUploader : IPhotoUploader
    {
        private readonly FlickrNet.Flickr _flickrService;
        private readonly FlickrPhotosetProvider _photosetProvider;
        private readonly FlickrLimitations _flickrLimitations;
        private readonly ITinyMessengerHub _messageHub;

        public FlickrPhotoUploader(FlickrNet.Flickr flickrService, FlickrPhotosetProvider photosetProvider, 
            FlickrLimitations flickrLimitations,
            ITinyMessengerHub messageHub)
        {
            _flickrService = flickrService;
            _photosetProvider = photosetProvider;
            _flickrLimitations = flickrLimitations;
            _messageHub = messageHub;
        }

        public void Upload(IEnumerable<DiskPhoto> photos)
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
                if (_flickrLimitations.IsValid(diskPhoto))
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

            var firstPhotoInAlbum = albumPhotos.First();

            FlickrPhoto albumCoverPhoto = null;
            Utils.Retry(() => albumCoverPhoto = UploadPhoto(firstPhotoInAlbum),
                ex => _messageHub.Publish(new PhotoUploadFailed(firstPhotoInAlbum, ex)),
                30);
            if (albumCoverPhoto == null) return;
            
            var photoSet = GetOrCreatePhotoSet(album, albumCoverPhoto);
            
            var restPhotosInAlbum = albumPhotos.Skip(1);

            foreach (var photo in restPhotosInAlbum)
            {
                _messageHub.Publish(new PhotoUploadStart(photo));

                FlickrPhoto flickrPhoto = null;
                Utils.Retry(() => flickrPhoto = UploadPhoto(photo),
                    ex => _messageHub.Publish(new PhotoUploadFailed(photo, ex)),
                    30);
                if (flickrPhoto == null) continue;
                
                AddPhotoToPhotoset(flickrPhoto.Id, photoSet.PhotosetId);

                _messageHub.Publish(new PhotoUploadEnd(photo));
            }

            _messageHub.Publish(new AlbumUploadEnd(album));
        }

        private Photoset GetOrCreatePhotoSet(Album album, FlickrPhoto albumCoverPhoto)
        {
            Photoset photoSet = null;
            if (!IsPhotosetOnFlickr(album.Title))
            {
                photoSet = _flickrService.PhotosetsCreate(album.Title, albumCoverPhoto.Id);
                //cover is automatically added to album
            }
            else
            {
                photoSet = GetPhotoSet(album.Title);
                AddPhotoToPhotoset(albumCoverPhoto.Id, photoSet.PhotosetId);
            }
            return photoSet;
        }

        private bool IsPhotosetOnFlickr(string title)
        {
            var photosets = _photosetProvider.GetPhotoSets();
            return photosets.Any(p => p.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase));
        }

        private Photoset GetPhotoSet(string title)
        {
            var photosets = _photosetProvider.GetPhotoSets();
            return photosets.Single(p => p.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase));
        }

        private void AddPhotoToPhotoset(string photoId, string photosetId)
        {
            Utils.Retry(() => _flickrService.PhotosetsAddPhoto(photosetId, photoId), ex => { }, 30);
        }

        private FlickrPhoto UploadPhoto(DiskPhoto photo)
        {
            using (var fs = new FileStream(photo.FilePath, FileMode.Open))
            {
                var photoId = _flickrService.UploadPicture(fs,
                    fileName: Path.GetFileName(photo.FilePath),
                    title: photo.Title,
                    description: "",
                    tags: "",
                    isPublic: false,
                    isFamily: true,
                    isFriend: false,
                    contentType: ContentType.Photo,
                    safetyLevel: SafetyLevel.None,
                    hiddenFromSearch: HiddenFromSearch.Hidden);

                var flickrPhoto = photo.ToFlickrPhoto();
                flickrPhoto.Id = photoId;
                return flickrPhoto;
            }
        }
    }
}