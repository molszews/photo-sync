using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyMessenger;

namespace PhotoBackup.Logic.Messages
{
    public class AlbumUploadEnd : ITinyMessage
    {
        public readonly Album Album;
        public object Sender { get; }

        public AlbumUploadEnd(Album album)
        {
            Album = album;
        }
    }
}
