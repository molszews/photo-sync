using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.GData.Client;

namespace PhotoBackup.Logic.GooglePhotos
{

    public class GoogleLimitations
    {
        private readonly FileHelper _fileHelper;
        public long MaxPhotoSize { get; } = 75L.MB();
        public long MaxVideoSize { get; } = 10L.GB();
        public long MaxItemsInAlbum { get; } = 2000;
        public long MaxNumberOfAlbums { get; } = 20000;

        public GoogleLimitations(FileHelper fileHelper)
        {
            _fileHelper = fileHelper;
        }

        public bool IsValid(FileInfo f)
        {
            var x = new List<int>();
            var y = Enumerable.Empty<int>();

            y = x;

            if (_fileHelper.IsPhotoFile(f))
            {
                return f.Length <= MaxPhotoSize;
            }
            else if (_fileHelper.IsVideoFile(f))
            {
                return false;
                return f.Length <= MaxVideoSize;
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