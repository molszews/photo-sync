using System;
using System.Linq;
using log4net;
using log4net.Config;
using PhotoBackup.Logic;
using PhotoBackup.Logic.Flickr;
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
//            rootDir = @"D:\Archiwum";
            var dirsToSkip = new[]
            {
                @"iMovie",
                @"Genealogia",
                @"olszak"
            };
            var diskPhotoProvider = new DiskPhotoProvider(rootDir, dirsToSkip);

            var messageHub = GetTinyMessengerHub();
            var bootstrap = new GoogleBootstrap(messageHub);
//            var bootstrap = new FlickrBootstrap(messageHub);

            var orchestrator = new Orchestrator(diskPhotoProvider, bootstrap.RemotePhotoProvider, bootstrap.Uploader);
            orchestrator.Start();
        }

        private static TinyMessengerHub GetTinyMessengerHub()
        {
            var messageHub = new TinyMessengerHub();
            messageHub.Subscribe<PhotoUploadStart>(m => { log.Info($"Photo upload started {m.Photo.Title}"); });
            messageHub.Subscribe<PhotoUploadEnd>(m => { log.Info($"Photo upload finished {m.Photo.Title}"); });
            messageHub.Subscribe<PhotoUploadSkipped>(m => { log.Warn($"Photo upload skipped {m.Photo.Title}"); });
            messageHub.Subscribe<PhotoUploadFailed>(m => { log.Error($"Photo upload failed {m.Photo.Title}", m._e); });
            messageHub.Subscribe<AlbumUploadStart>(m => { log.Info($"Album upload started {m.Album.Title}"); });
            messageHub.Subscribe<AlbumUploadEnd>(m => { log.Info($"Album upload finished {m.Album.Title}"); });
            messageHub.Subscribe<AlbumUploadFailed>(m => { log.Error($"Album upload failed {m.Album.Title}", m._e); });
            return messageHub;
        }

        private static void WipePhotos()
        {
            //            var messageHub1 = new TinyMessengerHub();
            //            messageHub1.Subscribe<PhotoDeleted>(m => { log.Info($"Photo deleted {m.Photo.Title}"); });
            //            var googlePhotoCleaner = new PhotoCleaner(messageHub1, googleService, servicePhotoProvider);
            //            googlePhotoCleaner.WipeAllPhotos();
            //            Console.ReadKey();
            //            return;
        }
    }
}