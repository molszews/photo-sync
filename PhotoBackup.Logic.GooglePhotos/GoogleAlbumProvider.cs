using System.Collections.Generic;
using System.Linq;
using Google.GData.Photos;

namespace PhotoBackup.Logic.GooglePhotos
{
    public interface IGoogleAlbumProvider
    {
        IEnumerable<Google.Picasa.Album> GetAlbums();
    }

    public class CachingGoogleAlbumProvider : IGoogleAlbumProvider
    {
        private readonly IGoogleAlbumProvider _provider;

        public CachingGoogleAlbumProvider(IGoogleAlbumProvider provider)
        {
            _provider = provider;
        }

        private IEnumerable<Google.Picasa.Album> _getAlbums;
        public IEnumerable<Google.Picasa.Album> GetAlbums()
        {
            if (_getAlbums == null)
            {
                _getAlbums = _provider.GetAlbums();
            }

            return _getAlbums;
        }
    }

    public class GoogleAlbumProvider : IGoogleAlbumProvider
    {
        private readonly PicasaService _service;

        public GoogleAlbumProvider(PicasaService service)
        {
            _service = service;
        }

        public IEnumerable<Google.Picasa.Album> GetAlbums()
        {
            var query = new AlbumQuery(PicasaQuery.CreatePicasaUri("default"));
            var feed = _service.Query(query);
            return feed.Entries.Select(e => new Google.Picasa.Album { AtomEntry = e });
        }
    }
}