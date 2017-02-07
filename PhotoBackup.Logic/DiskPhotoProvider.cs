using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhotoBackup.Logic
{
    public class DiskPhotoProvider
    {
        public string RootDir { get; }
        
        private readonly IEnumerable<string> _pathsToSkip;

        public DiskPhotoProvider(string rootDir, IEnumerable<string> dirsToSkip = null)
        {
            RootDir = rootDir;
            _pathsToSkip = dirsToSkip ?? Enumerable.Empty<string>();
        }

        public IEnumerable<DiskPhoto> GetPhotos()
        {
            var allowedFilesByDir = ListImageFilesInFolder(RootDir)
                .Where(IsNotInDirToSkip)
                .GroupBy(path => path.Substring(0, path.LastIndexOf(@"\")));

            var photos = allowedFilesByDir.SelectMany(g =>
            {
                var dirPath = g.Key;
                var album = new Album
                {
                    Title = DirToAlbumName(dirPath)
                };

                return g.Select(photo => new DiskPhoto
                {
                    Album = album,
                    Title = Path.GetFileNameWithoutExtension(photo),
                    FilePath = photo,
                    FileSize = new FileInfo(photo).Length
                });
            });
            return photos;
        }

//        public IEnumerable<IPhoto> GetPhotos(IEnumerable<Album> albums)
//        {
//            var albumNames = albums.Select(a => a.Title);
//            var allPhoto = GetPhotos();
//            return allPhoto.Where(p => albumNames.Contains(p.Album.Title, StringComparer.InvariantCultureIgnoreCase));
//        }

        private bool IsNotInDirToSkip(string filePath)
        {
            return false == _pathsToSkip.Any(pathToSkip => filePath.StartsWith(Path.Combine(RootDir, pathToSkip), StringComparison.InvariantCultureIgnoreCase));
        }

        private IEnumerable<string> ListImageFilesInFolder(string rootDir)
        {
            var allfiles = Directory.GetFiles(rootDir, "*.*", SearchOption.AllDirectories);
            var allowedFiles = allfiles.Where(f => FileHelper.IsPhotoFile(f) || FileHelper.IsVideoFile(f));
            return allowedFiles;
        }

        private string DirToAlbumName(string path)
        {
            return path.Replace(RootDir, "").TrimStart('\\').TrimEnd('\\').Replace("\\", " - ");
        }
    }
}
