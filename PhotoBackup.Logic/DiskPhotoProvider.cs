using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhotoBackup.Logic
{
    public class FileHelper
    {
        private readonly string[] _photosExtensions = { ".jpg", ".png" };
        private readonly string[] _videosExtensions = { ".mts", ".mov", ".avi", ".mpg" };

        public bool IsVideoFile(FileInfo file)
        {
            return _videosExtensions.Contains(file.Extension, StringComparer.InvariantCultureIgnoreCase);
        }

        public bool IsPhotoFile(FileInfo file)
        {
            return _photosExtensions.Contains(file.Extension, StringComparer.InvariantCultureIgnoreCase);
        }
    }

    public class DiskPhotoProvider : IPhotoProvider
    {
        private readonly FileHelper _fileHelper;
        private readonly string _rootDir;
        private readonly IEnumerable<string> _pathsToSkip;
        

        public DiskPhotoProvider(FileHelper fileHelper, string rootDir, IEnumerable<string> dirsToSkip = null)
        {
            _fileHelper = fileHelper;
            _rootDir = rootDir;
            _pathsToSkip = (dirsToSkip ?? Enumerable.Empty<string>()).Select(d => Path.Combine(_rootDir, d));
        }

        public IEnumerable<IPhoto> GetPhotos()
        {
            var allowedFilesByDir = ListImageFilesInFolder(_rootDir)
                .GroupBy(f => f.Directory.FullName, f => f)
                .Where(IsNotInDirToSkip);

            var photos = allowedFilesByDir.SelectMany(g =>
            {
                var dirPath = g.Key;
                var album = new Album()
                {
                    Title = DirToAlbumName(dirPath),
                    Url = dirPath
                };
                return g.Select(photo => new DiskPhoto()
                {
                    Album = album,
//                    Title = Path.GetFileName(photo.FullName),
                    Title = Path.GetFileNameWithoutExtension(photo.FullName),
                    Url = photo.FullName
                });
            });
            return photos;
        }

        public IEnumerable<IPhoto> GetPhotos(IEnumerable<Album> albums)
        {
            var albumNames = albums.Select(a => a.Title);
            var allPhoto = GetPhotos();
            return allPhoto.Where(p => albumNames.Contains(p.Album.Title, StringComparer.InvariantCultureIgnoreCase));
        }

        private bool IsNotInDirToSkip(IGrouping<string, FileInfo> g)
        {
            return !_pathsToSkip.Any(pathToSkip => g.Key.StartsWith(pathToSkip, StringComparison.InvariantCultureIgnoreCase));
        }

        private IEnumerable<FileInfo> ListImageFilesInFolder(string rootDir)
        {
            var allfiles = Directory.GetFiles(rootDir, "*.*", SearchOption.AllDirectories);
            var fileInfos = allfiles.Select(f => new FileInfo(f));
            var allowedFiles = fileInfos.Where(f => _fileHelper.IsPhotoFile(f) || _fileHelper.IsVideoFile(f));
            return allowedFiles;
        }

        private string DirToAlbumName(string path)
        {
            return path.Replace(_rootDir, "").TrimStart('\\').Replace("\\", " - ");
        }
    }
}
