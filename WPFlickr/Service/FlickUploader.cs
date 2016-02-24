using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using FlickrNet;

namespace WPFlickr.Service
{
    public class FlickUploader
    {
        private Flickr f;
        private string _placeholderPhotoId;

        public void UploadAlbums(string baseFolder, IEnumerable<string> albums)
        {
            f = FlickrManager.GetAuthInstance();
            _placeholderPhotoId = f.UploadPicture("pimpek.png");

            var stopwatch = Stopwatch.StartNew();
            foreach (var albumName in albums)
            {
                var albumFolder = Path.Combine(baseFolder, albumName);
                var allFiles = Directory.GetFiles(albumFolder, "*.*", SearchOption.TopDirectoryOnly);
                UploadAlbum(albumName, allFiles);
            }
            stopwatch.Stop();

            f.PhotosDelete(_placeholderPhotoId);
        }

        private void UploadAlbum(string albumName, IEnumerable<string> filesPaths)
        {
            var primaryPhotoId = _placeholderPhotoId;
            var photoSet = f.PhotosetsCreate(albumName, primaryPhotoId);

            var photoIds = filesPaths.AsParallel().Select(file =>
            {
                try
                {
                    using (var fs = new FileStream(file, FileMode.Open))
                    {
                        var photoId = f.UploadPicture(fs,
                            fileName: Path.GetFileName(file),
                            title: Path.GetFileNameWithoutExtension(file),
                            description: "",
                            tags: "",
                            isPublic: false,
                            isFamily: true,
                            isFriend: false,
                            contentType: ContentType.Photo,
                            safetyLevel: SafetyLevel.None,
                            hiddenFromSearch: HiddenFromSearch.Hidden);
                        f.PhotosetsAddPhoto(photoSet.PhotosetId, photoId);

                        return photoId;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(file + "\n" + e.Message);
                    throw;
                }
            }).ToList();


            f.PhotosetsSetPrimaryPhoto(photoSet.PhotosetId, photoIds.First());
            f.PhotosetsRemovePhoto(photoSet.PhotosetId, primaryPhotoId);
        }
    }
}