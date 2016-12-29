using System.Collections.Generic;

namespace PhotoBackup.Logic
{
    public interface IPhotoProvider
    {
        IEnumerable<IPhoto> GetPhotos();
        IEnumerable<IPhoto> GetPhotos(IEnumerable<Album> albums);
    }
}