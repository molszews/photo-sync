using System;
using TinyMessenger;

namespace PhotoBackup.Logic.Messages
{
    public class PhotoUploadSkipped : ITinyMessage
    {
        public readonly DiskPhoto Photo;

        public object Sender { get; }

        public PhotoUploadSkipped(DiskPhoto photo)
        {
            Photo = photo;
        }
    }
}