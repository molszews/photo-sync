using TinyMessenger;

namespace PhotoBackup.Logic.GooglePhotos
{
    public class GoogleBootstrap : IBootstrap
    {
        public IPhotoProvider RemotePhotoProvider { get; }
        public IPhotoUploader Uploader { get; }
        
        public GoogleBootstrap(ITinyMessengerHub messageHub)
        {
            var limitations = new GoogleLimitations();
            var googleService = GoogleAuth.GetInstance();
            var googleAlbumsProvider = new GoogleAlbumProvider(googleService);
            var cachingGoogleAlbumProvider = new CachingGoogleAlbumProvider(googleAlbumsProvider);
            RemotePhotoProvider = new GooglePhotoProvider(googleService, cachingGoogleAlbumProvider);
            Uploader = new GooglePhotoUploader(googleService, cachingGoogleAlbumProvider, limitations, messageHub);
        }
    }
}
