namespace PhotoBackup.Logic.Flickr
{
    public class FlickrLimitations
    {
        public long MaxPhotoSize { get; } = 200L.MB();
        public long MaxVideoSize { get; } = 1L.GB();

        public bool IsValid(DiskPhoto diskPhoto)
        {
            if (FileHelper.IsPhotoFile(diskPhoto.FilePath))
            {
                return diskPhoto.FileSize <= MaxPhotoSize;
            }
            else if (FileHelper.IsVideoFile(diskPhoto.FilePath))
            {
                return diskPhoto.FileSize <= MaxVideoSize;
            }
            return false;
        }
    }

    public static class BytesExtensions
    {
        public static long KB(this long kiloBytes)
        {
            return kiloBytes * 1024;
        }

        public static long MB(this long megaBytes)
        {
            return (megaBytes*1024).KB();
        }

        public static long GB(this long gigaBytes)
        {
            return (gigaBytes * 1024).MB();
        }
    }
}