namespace PhotoBackup.Logic.Flickr
{
    public class FlickrPhoto : IPhoto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public Album Album { get; set; }
        public string Url { get; set; }
    }

    public static class FlickrPhotoExtensions
    {
        public static FlickrPhoto ToFlickrPhoto(this DiskPhoto diskPhoto)
        {
            return new FlickrPhoto
            {
                Title = diskPhoto.Title,
                Album = diskPhoto.Album
            };
        }
    }
}