using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBackup.Logic
{
    public interface IPhoto
    {
        string Title { get; set; }
        Album Album { get; set; }
        string Url { get; set; }
    }

    public class DiskPhoto : IPhoto
    {
        public string Title { get; set; }

        public Album Album { get; set; }

        public string Url { get; set; }
    }
}
