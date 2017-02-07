using System.Collections.Generic;
using System.Linq;

namespace PhotoBackup.Logic
{
    public static class SyncService
    {
        public static IEnumerable<T> Except<T>(this IEnumerable<T> localPhotos, IEnumerable<IPhoto> remotePhotos) where T : IPhoto
        {
            var remoteAsStringList = remotePhotos.Select(p => p.Title + "\\" + p.Album.Title).ToList();
            foreach (var localPhoto in localPhotos)
            {
                var diskPhotoAsString = localPhoto.Title + "\\" + localPhoto.Album.Title;
                if (false == remoteAsStringList.Contains(diskPhotoAsString))
                    yield return localPhoto;
            }
        }
    }
}