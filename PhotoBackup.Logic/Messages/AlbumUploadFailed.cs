using System;
using TinyMessenger;

namespace PhotoBackup.Logic.Messages
{
    public class AlbumUploadFailed : ITinyMessage
    {
        public readonly Album Album;
        public readonly Exception _e;

        public object Sender { get; }

        public AlbumUploadFailed(Album album, Exception e)
        {
            Album = album;
            _e = e;
        }
    }
}