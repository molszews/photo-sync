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
using TinyMessenger;

namespace PhotoBackup.Logic.Flickr
{
    public class FlickrPhotoUploader : IPhotoUploader
    {
        private readonly FlickrNet.Flickr _flickrService;
        private readonly FlickrPhotosetProvider _photosetProvider;
        private readonly FileHelper _fileHelper;
        private readonly FlickrLimitations _flickrLimitations;
        private readonly ITinyMessengerHub _messageHub;

        public FlickrPhotoUploader(FlickrNet.Flickr flickrService, FlickrPhotosetProvider photosetProvider, 
            FileHelper fileHelper, 
            FlickrLimitations flickrLimitations,
            ITinyMessengerHub messageHub)
        {
            _flickrService = flickrService;
            _photosetProvider = photosetProvider;
            _fileHelper = fileHelper;
            _flickrLimitations = flickrLimitations;
            _messageHub = messageHub;
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
                if (_flickrLimitations.IsValid(fileInfo))
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
            var albumCoverPhoto = UploadPhotoWithRetry(firstPhotoInAlbum);

            var photoSet = GetOrCreatePhotoSet(album, albumCoverPhoto);

            var uploadedPhotos = new ConcurrentBag<FlickrPhoto>();

            var restPhotosInAlbum = albumPhotos.Skip(1);

            Parallel.ForEach(restPhotosInAlbum, new ParallelOptions {MaxDegreeOfParallelism = 8}, photo =>
            {
                _messageHub.Publish(new PhotoUploadStart(photo));
                var flickrPhoto = UploadPhotoWithRetry(photo);
                AddPhotoToPhotoset(flickrPhoto.Id, photoSet.PhotosetId);
                uploadedPhotos.Add(flickrPhoto);

                _messageHub.Publish(new PhotoUploadEnd(photo));
            });

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

        private void AddPhotoToPhotoset(string photoId, string photosetId, int retriesLeft = 3)
        {
            try
            {
                _flickrService.PhotosetsAddPhoto(photosetId, photoId);
            }
            catch (Exception)
            {
                if (retriesLeft > 0)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    AddPhotoToPhotoset(photoId, photosetId, --retriesLeft);
                }

//                throw;
            }

        }

        private FlickrPhoto UploadPhotoWithRetry(DiskPhoto photo, int retriesLeft = 3)
        {
            try
            {
                return UploadPhoto(photo);
            }
            catch (Exception e)
            {
                _messageHub.Publish(new PhotoUploadFailed(photo, e, retriesLeft));

                if (retriesLeft > 0)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    return UploadPhotoWithRetry(photo, --retriesLeft);
                }

                return new FlickrPhoto() {Id = "-1"};
//                throw;
            }
        }

        private FlickrPhoto UploadPhoto(DiskPhoto photo)
        {
            using (var fs = new FileStream(photo.Url, FileMode.Open))
            {
                var photoId = _flickrService.UploadPicture(fs,
                    fileName: Path.GetFileName(photo.Url),
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