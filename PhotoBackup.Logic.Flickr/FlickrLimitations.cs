using System.Collections.Generic;
using System.IO;

namespace PhotoBackup.Logic.Flickr
{
    public class FlickrLimitations
    {
        private readonly FileHelper _fileHelper;
        public long MaxPhotoSize { get; } = 200L.MB();
        public long MaxVideoSize { get; } = 1L.GB();

        public FlickrLimitations(FileHelper fileHelper)
        {
            _fileHelper = fileHelper;
        }

        public bool IsValid(FileInfo f)
        {
            if (_fileHelper.IsPhotoFile(f))
            {
                return f.Length <= MaxPhotoSize;
            } else if (_fileHelper.IsVideoFile(f))
            {
                return f.Length <= MaxVideoSize;
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