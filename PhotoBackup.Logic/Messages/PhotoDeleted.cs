using TinyMessenger;

namespace PhotoBackup.Logic.Messages
{
    public class PhotoDeleted : ITinyMessage
    {
        public readonly IPhoto Photo;
        public object Sender { get; }

        public PhotoDeleted(IPhoto photo)
        {
            Photo = photo;
        }
    }
}