using System.Collections.Generic;

namespace PhotoBackup.Logic
{
    public interface IPhotoUploader
    {
        void Upload(IEnumerable<DiskPhoto> photos);
    }
}