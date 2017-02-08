using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using PhotoBackup.Logic.Messages;

namespace PhotoBackup.Logic
{
    public class Orchestrator
    {
        private readonly IPhotoUploader _photoUploader;
        private readonly DiskPhotoProvider _diskPhotoProvider;
        private readonly IPhotoProvider _remotePhotoProvider;

        private static readonly ILog log = LogManager.GetLogger(typeof(Orchestrator));

        public Orchestrator(DiskPhotoProvider diskPhotoProvider, IPhotoProvider remotePhotoProvider, IPhotoUploader photoUploader)
        {
            _photoUploader = photoUploader;
            _diskPhotoProvider = diskPhotoProvider;
            _remotePhotoProvider = remotePhotoProvider;
        }

        public void Start()
        {
            var diskPhotos = _diskPhotoProvider.GetPhotos().ToList();
            var diskAlbums = diskPhotos.Select(p => p.Album).Distinct().ToList();

            DisplayDiskPhotos(diskPhotos);

            var remotePhotos = _remotePhotoProvider.GetPhotos(diskAlbums).ToList();

            DisplayRemotePhotos(remotePhotos);

            var missingPhotos = diskPhotos.Except(remotePhotos).ToList();

            DisplayPhotosToUpload(missingPhotos);

            Console.ReadKey();
            Console.Clear();
            
            _photoUploader.Upload(missingPhotos);

            log.Info("Upload finished");
            Console.ReadKey();
        }

        private static void DisplayPhotosToUpload(List<DiskPhoto> missingPhotos)
        {
            log.Info("-----------------------------------------------------------");
            log.Info($"Photos to upload (first 50 of {missingPhotos.Count}):");

            foreach (var photo in missingPhotos.Take(50))
            {
                log.Info($"\t{photo.Album.Title} - {photo.Title}");
            }
        }

        private static void DisplayRemotePhotos(List<IPhoto> remotePhotos)
        {
            log.Info("-----------------------------------------------------------");
            log.Info($"Remote photos (first 50 of {remotePhotos.Count}):");

            foreach (var photo in remotePhotos.Take(50))
            {
                log.Info($"\t{photo.Album.Title} - {photo.Title}");
            }
        }

        private void DisplayDiskPhotos(List<DiskPhoto> diskPhotos)
        {
            log.Info($"Indexing photos in folder {_diskPhotoProvider.RootDir} (first 50 of {diskPhotos.Count}):");
            foreach (var photo in diskPhotos.Take(50))
            {
                log.Info($"\t{photo.Album.Title} - {photo.Title}");
            }
        }
    }
}
