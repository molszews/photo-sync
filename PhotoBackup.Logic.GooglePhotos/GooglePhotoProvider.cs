using System;
using System.Collections.Generic;
using System.Linq;
using Google.GData.Photos;
using Google.Picasa;
using PicasaAlbum = Google.Picasa.Album;

namespace PhotoBackup.Logic.GooglePhotos
{
    public class GooglePhotoProvider : IPhotoProvider
    {
        private readonly PicasaService _service;
        private readonly IGoogleAlbumProvider _googleAlbumProvider;

        public GooglePhotoProvider(PicasaService service, IGoogleAlbumProvider googleAlbumProvider)
        {
            _service = service;
            _googleAlbumProvider = googleAlbumProvider;
        }

        public IEnumerable<IPhoto> GetPhotos()
        {
            return GetPhotos(null);
        }

        private IEnumerable<PicasaAlbum> FilterAlbums(IEnumerable<Album> albums)
        {
            var googleAlbums = _googleAlbumProvider.GetAlbums();
            if (albums == null) return googleAlbums;
            return googleAlbums.Where(
                ga => albums.Select(a => a.Title).Contains(ga.Title, StringComparer.InvariantCultureIgnoreCase));
        }

        public IEnumerable<IPhoto> GetPhotos(IEnumerable<Album> albums)
        {
            var googleAlbums = FilterAlbums(albums);
            return googleAlbums.SelectMany(googleAlbum =>
            {
                var query = new PhotoQuery(PicasaQuery.CreatePicasaUri("default", googleAlbum.Id));
                var feed = _service.Query(query);
                return feed.Entries.Select(e => new Photo {AtomEntry = e}).Select(gp => new GooglePhoto
                {
                    Album = new Album {Title = googleAlbum.Title},
                    Title = gp.Title
                });
            });
        }
    }
}