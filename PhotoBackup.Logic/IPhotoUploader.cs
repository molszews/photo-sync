using System.Collections.Generic;

namespace PhotoBackup.Logic
{
    public interface IPhotoUploader
    {
        void UploadPhotos(IEnumerable<DiskPhoto> photos);
    }
}