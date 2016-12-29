using System.Collections.ObjectModel;
using WPFlickr.ViewModel;

namespace WPFlickr.DesignTimeData
{
    public class AlbumList
    {
        public ObservableCollection<Album> Albums { get; set; } = new ObservableCollection<Album>();

        public AlbumList()
        {
            Albums.Add(new Album {IsUploaded = false, Title = "Super Album"});
            Albums.Add(new Album {IsUploaded = true, Title = "Słaby Album"});
        }
    }
}