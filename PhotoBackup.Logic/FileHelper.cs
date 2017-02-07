using System;
using System.Linq;

namespace PhotoBackup.Logic
{
    public static class FileHelper
    {
        private static readonly string[] PhotosExtensions = { ".jpg", ".png" };
        private static readonly string[] VideosExtensions = { ".mts", ".mov", ".avi", ".mpg" };

        public static bool IsVideoFile(string fileName)
        {
            return VideosExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsPhotoFile(string fileName)
        {
            return PhotosExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }
    }
}