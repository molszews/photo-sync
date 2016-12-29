using System;
using System.Linq;
using log4net;
using log4net.Config;
using PhotoBackup.Logic;
using PhotoBackup.Logic.GooglePhotos;
using PhotoBackup.Logic.Messages;
using TinyMessenger;

[assembly: XmlConfigurator(Watch = true)]

namespace FlickrUploader
{
    internal class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            var rootDir = @"H:\Archiwum";
//            rootDir = @"D:\Archiwum-foto";
            var dirsToSkip = new[]
            {
//                @"2016\18-stka Macieja w Karsinie",
                @"Genealogia",
//                @"2016"
            };

            var fileHelper = new FileHelper();
            var diskPhotoProvider = new DiskPhotoProvider(fileHelper, rootDir, dirsToSkip);

            var googleService = GoogleAuth.GetInstance();
            var googleAlbumsProvider = new GoogleAlbumProvider(googleService);
            var cachingGoogleAlbumProvider = new CachingGoogleAlbumProvider(googleAlbumsProvider);
            var servicePhotoProvider = new GooglePhotoProvider(googleService, cachingGoogleAlbumProvider);

//            var flickrService = FlickrAuth.GetAuthInstance();
//            var flickrPhotosetProvider = new FlickrPhotosetProvider(flickrService);
//            var cachingFlickrPhotosetProvider = new CachingFlickrPhotosetProvider(flickrPhotosetProvider);
//            var servicePhotoProvider = new FlickrPhotoProvider(flickrService, cachingFlickrPhotosetProvider);

            var diskPhotos = diskPhotoProvider.GetPhotos().ToList();
            var diskAlbums = diskPhotos.Select(p => p.Album).Distinct().ToList();

//            log.Info($"Indexing photos in folder: {rootDir}");
//            foreach (var photo in diskPhotos)
//            {
//                log.Info($"\t{photo.Album.Title} - {photo.Title}");
//            }

            log.Info("-----------------------------------------------------------");
            log.Info("Remote photos:");

            var flickrPhotos = servicePhotoProvider.GetPhotos(diskAlbums).ToList();
//            foreach (var photo in flickrPhotos)
//            {
//                log.Info($"\t{photo.Album.Title} - {photo.Title}");
//            }

            log.Info("-----------------------------------------------------------");
            log.Info("Photos to upload:");

            var syncService = new SyncService();
            var missingPhotos = syncService.FindMissingPhotos(diskPhotos, flickrPhotos).ToList();

            foreach (var photo in missingPhotos)
            {
                log.Info($"\t{photo.Album.Title} - {photo.Title}");
            }

            Console.Clear();
            log.Info("-----------------------------------------------------------");
            log.Info("Upload started");

            var messageHub = new TinyMessengerHub();
            messageHub.Subscribe<PhotoUploadStart>(m => { log.Info($"Photo upload started {m.Photo.Title}"); });
            messageHub.Subscribe<PhotoUploadEnd>(m => { log.Info($"Photo upload finished {m.Photo.Title}"); });
            messageHub.Subscribe<PhotoUploadSkipped>(m => { log.Warn($"Photo upload skipped {m.Photo.Title}"); });
            messageHub.Subscribe<PhotoUploadFailed>(m => { log.Error($"Photo upload failed {m.Photo.Title}", m._e); });
            messageHub.Subscribe<AlbumUploadStart>(m => { log.Info($"Album upload started {m.Album.Title}"); });
            messageHub.Subscribe<AlbumUploadEnd>(m => { log.Info($"Album upload finished {m.Album.Title}"); });
            messageHub.Subscribe<AlbumUploadFailed>(m => { log.Error($"Album upload failed {m.Album.Title}", m._e); });

//            var flickrLimitations = new FlickrLimitations(fileHelper);
//            var flickrUploader = new FlickrPhotoUploader(flickrService, flickrPhotosetProvider, fileHelper, flickrLimitations, messageHub);
//                        flickrUploader.UploadPhotos(missingPhotos.Cast<DiskPhoto>());

            var googleLimitations = new GoogleLimitations(fileHelper);
            var uploader = new GooglePhotoUploader(googleService, cachingGoogleAlbumProvider, messageHub,
                googleLimitations);
            uploader.UploadPhotos(missingPhotos.Cast<DiskPhoto>());

            log.Info("Upload finished");
            Console.ReadKey();
        }
    }
}