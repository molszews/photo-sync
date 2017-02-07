namespace PhotoBackup.Logic.GooglePhotos
{
    public class GoogleLimitations
    {
        public long MaxPhotoSize { get; } = 75L.MB();
        public long MaxVideoSize { get; } = 10L.GB();
        public long MaxItemsInAlbum { get; } = 2000;
        public long MaxNumberOfAlbums { get; } = 20000;
        

        public bool IsValid(DiskPhoto diskPhoto)
        {
            if (FileHelper.IsPhotoFile(diskPhoto.FilePath))
            {
                return diskPhoto.FileSize <= MaxPhotoSize;
            }
            else if (FileHelper.IsVideoFile(diskPhoto.FilePath))
            {
//                return false;
                return diskPhoto.FileSize <= MaxVideoSize;
            }
            return false;
        }
    }

    public static class BytesExtensions
    {
        public static long KB(this long kiloBytes)
        {
            return kiloBytes * 1024L;
        }

        public static long MB(this long megaBytes)
        {
            return (megaBytes*1024L).KB();
        }

        public static long GB(this long gigaBytes)
        {
            return (gigaBytes * 1024L).MB();
        }
    }
}