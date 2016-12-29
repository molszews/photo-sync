using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyMessenger;

namespace PhotoBackup.Logic.Messages
{
    public class AlbumUploadStart : ITinyMessage
    {
        public readonly Album Album;
        public object Sender { get; }

        public AlbumUploadStart(Album album)
        {
            Album = album;
        }
    }
}
