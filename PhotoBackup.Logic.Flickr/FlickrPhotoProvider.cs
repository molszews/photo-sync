using System;
using System.Collections.Generic;
using System.Linq;
using FlickrNet;

namespace PhotoBackup.Logic.Flickr
{
    public class FlickrPhotoProvider : IPhotoProvider
    {
        private readonly FlickrNet.Flickr _flickrService;
        private readonly IFlickrPhotosetProvider _photosetProvider;

        public FlickrPhotoProvider(FlickrNet.Flickr flickrService, IFlickrPhotosetProvider photosetProvider)
        {
            _flickrService = flickrService;
            _photosetProvider = photosetProvider;
        }

        public IEnumerable<IPhoto> GetPhotos()
        {
            var photoSets = _photosetProvider.GetPhotoSets();
            return photoSets.SelectMany(GetDomainPhotosInPhotoSet);
        }

        public IEnumerable<IPhoto> GetPhotos(IEnumerable<Album> albums)
        {
            var albumTitles = albums.Select(a => a.Title);
            var photoSets = _photosetProvider.GetPhotoSets().Where(p => albumTitles.Contains(p.Title, StringComparer.InvariantCultureIgnoreCase));
            return photoSets.SelectMany(GetDomainPhotosInPhotoSet);
        }

        private IEnumerable<IPhoto> GetDomainPhotosInPhotoSet(Photoset photoSet)
        {
            var domainAlbum = new Album()
            {
                Url = photoSet.Url, Title = photoSet.Title
            };

            var flickrPhotos = GetPhotosInPhotoSet(photoSet);
            return flickrPhotos.Select(p => new DiskPhoto()
            {
                Url = p.OriginalUrl, Title = p.Title, Album = domainAlbum
            });
        }
        
        private IEnumerable<FlickrNet.Photo> GetPhotosInPhotoSet(Photoset photoSet)
        {
            var pageNo = 1;
            var perPage = 500;
            var pagesNo = 1; //initial value

            for (var page = pageNo; page <= pagesNo; page++)
            {
                var photos = _flickrService.PhotosetsGetPhotos(photoSet.PhotosetId, PhotoSearchExtras.None,
                    PrivacyFilter.None, page, perPage);

                foreach (var photo in photos)
                {
                    yield return photo;
                }

                pagesNo = photos.Pages;
            }
        }
    }
}
