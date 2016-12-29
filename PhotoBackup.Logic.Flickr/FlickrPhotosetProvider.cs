using System;
using System.Collections.Generic;
using System.Linq;
using FlickrNet;

namespace PhotoBackup.Logic.Flickr
{
    public interface IFlickrPhotosetProvider
    {
        IEnumerable<Photoset> GetPhotoSets();
    }

    public class CachingFlickrPhotosetProvider : IFlickrPhotosetProvider
    {
        private readonly IFlickrPhotosetProvider _provider;

        public CachingFlickrPhotosetProvider(IFlickrPhotosetProvider provider)
        {
            _provider = provider;
        }

        private IEnumerable<Photoset> _getPhotosets;
        public IEnumerable<Photoset> GetPhotoSets()
        {
            if (_getPhotosets == null)
            {
                _getPhotosets = _provider.GetPhotoSets();
            }

            return _getPhotosets;
        }
    }

    public class FlickrPhotosetProvider : IFlickrPhotosetProvider
    {
        private readonly FlickrNet.Flickr _flickrService;

        public FlickrPhotosetProvider(FlickrNet.Flickr flickrService)
        {
            _flickrService = flickrService;
        }

        public IEnumerable<Photoset> GetPhotoSets()
        {
            var allPhotoSets = _flickrService.PhotosetsGetList();
            var firstPageNo = allPhotoSets.Page;
            var pagesNo = allPhotoSets.Pages;
            var perPage = allPhotoSets.PerPage;
            for (var page = firstPageNo; page <= pagesNo; page++)
            {
                var photoSets = _flickrService.PhotosetsGetList(page, perPage);
                foreach (var photoSet in photoSets)
                {
                    if(!photoSet.Title.Equals("Auto Upload", StringComparison.InvariantCultureIgnoreCase))
                        yield return photoSet;
                }
            }
        }
    }
}