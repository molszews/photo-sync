namespace PhotoBackup.Logic
{
    public interface IPhoto
    {
        string Title { get; set; }
        Album Album { get; set; }
    }

    public class DiskPhoto : IPhoto
    {
        public string Title { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public Album Album { get; set; }
    }
}
