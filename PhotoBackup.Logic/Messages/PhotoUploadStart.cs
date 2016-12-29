using TinyMessenger;

namespace PhotoBackup.Logic.Messages
{
    public class PhotoUploadStart : ITinyMessage
    {
        public readonly DiskPhoto Photo;
        public object Sender { get; }

        public PhotoUploadStart(DiskPhoto photo)
        {
            Photo = photo;
        }
    }
}