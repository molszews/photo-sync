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
            var albums = _googleAlbumProvider.GetAlbums();
            return GetPhotos(albums.Select(ga => new Album
            {
                Title = ga.Title,
                Url = ga.Id
            }));
        }

        public IEnumerable<IPhoto> GetPhotos(IEnumerable<Album> albums)
        {
            var googleAlbums = _googleAlbumProvider.GetAlbums();
            
            return albums.SelectMany(a =>
            {
                var googleAlbum =
                    googleAlbums.SingleOrDefault(ga => ga.Title.Equals(a.Title, StringComparison.InvariantCultureIgnoreCase));
                if (googleAlbum == null) return Enumerable.Empty<GooglePhoto>();

                var query = new PhotoQuery(PicasaQuery.CreatePicasaUri("default", googleAlbum.Id));
                var feed = _service.Query(query);
                return feed.Entries.Select(e => new Photo {AtomEntry = e}).Select(gp => new GooglePhoto
                {
                    Album = a,
                    Title = gp.Title,
                    Url = gp.PhotoUri.AbsolutePath
                });
            });
        }
    }
}