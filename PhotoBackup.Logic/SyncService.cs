using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PhotoBackup.Logic
{
    public class SyncService
    {
        class PhotoComparer : IEqualityComparer<IPhoto>
        {
            public bool Equals(IPhoto x, IPhoto y)
            {
                return x.Title == y.Title && x.Album.Title == y.Album.Title;
            }

            public int GetHashCode(IPhoto obj)
            {
                return (obj.Title + "\\" + obj.Album.Title).GetHashCode();
            }
        }

        public IEnumerable<IPhoto> FindMissingPhotos(IEnumerable<IPhoto> local, IEnumerable<IPhoto> remote)
        {
            return local.Except(remote, new PhotoComparer());
        }
    }
}