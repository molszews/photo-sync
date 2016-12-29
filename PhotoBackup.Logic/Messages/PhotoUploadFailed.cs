using System;
using TinyMessenger;

namespace PhotoBackup.Logic.Messages
{
    public class PhotoUploadFailed : ITinyMessage
    {
        public readonly DiskPhoto Photo;
        public readonly Exception _e;
        public readonly int _retriesLeft;

        public object Sender { get; }

        public PhotoUploadFailed(DiskPhoto photo, Exception e, int retriesLeft)
        {
            Photo = photo;
            _e = e;
            _retriesLeft = retriesLeft;
        }
    }
}