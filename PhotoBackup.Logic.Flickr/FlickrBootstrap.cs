using TinyMessenger;

namespace PhotoBackup.Logic.Flickr
{
    public class FlickrBootstrap : IBootstrap
    {
        public IPhotoProvider RemotePhotoProvider { get; }
        public IPhotoUploader Uploader { get; }

        public FlickrBootstrap(ITinyMessengerHub messageHub)
        {
            var flickrService = FlickrAuth.GetAuthInstance();
            var flickrPhotosetProvider = new FlickrPhotosetProvider(flickrService);
            var cachingFlickrPhotosetProvider = new CachingFlickrPhotosetProvider(flickrPhotosetProvider);
            RemotePhotoProvider = new FlickrPhotoProvider(flickrService, cachingFlickrPhotosetProvider);
            var flickrLimitations = new FlickrLimitations();
            Uploader = new FlickrPhotoUploader(flickrService, flickrPhotosetProvider, flickrLimitations, messageHub);
        }
    }
}