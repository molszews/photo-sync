namespace PhotoBackup.Logic.GooglePhotos
{
    public class GooglePhoto : IPhoto
    {
        public string Title { get; set; }
        public Album Album { get; set; }
    }

    public static class GooglePhotoExtensions
    {
        public static GooglePhoto ToGooglePhoto(this DiskPhoto diskPhoto)
        {
            return new GooglePhoto
            {
                Title = diskPhoto.Title,
                Album = diskPhoto.Album
            };
        }
    }
}