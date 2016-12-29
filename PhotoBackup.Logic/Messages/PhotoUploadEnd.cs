using TinyMessenger;

namespace PhotoBackup.Logic.Messages
{
    public class PhotoUploadEnd : ITinyMessage
    {
        public readonly DiskPhoto Photo;
        public object Sender { get; }

        public PhotoUploadEnd(DiskPhoto photo)
        {
            Photo = photo;
        }
    }
}