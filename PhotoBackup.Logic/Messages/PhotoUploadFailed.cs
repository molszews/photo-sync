using System;
using TinyMessenger;

namespace PhotoBackup.Logic.Messages
{
    public class PhotoUploadFailed : ITinyMessage
    {
        public readonly DiskPhoto Photo;
        public readonly Exception _e;

        public object Sender { get; }

        public PhotoUploadFailed(DiskPhoto photo, Exception e)
        {
            Photo = photo;
            _e = e;
        }
    }
}