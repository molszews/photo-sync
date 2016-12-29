using System.IO;
using System.Threading.Tasks;
using FlickrNet;

namespace PhotoBackup.Logic.Flickr
{
    public class FlickrAsyncApi
    {
        private readonly FlickrNet.Flickr _flickrService;

        public FlickrAsyncApi(FlickrNet.Flickr flickrService)
        {
            _flickrService = flickrService;
        }

        public Task<string> UploadPicture(Stream stream, string fileName, string title, string description, string tags,
            bool isPublic, bool isFamily, bool isFriend, ContentType contentType, SafetyLevel safetyLevel,
            HiddenFromSearch hiddenFromSearch)
        {
            var result = new TaskCompletionSource<string>();
            _flickrService.UploadPictureAsync(stream, fileName, title, description, tags, isPublic, isFamily, isFriend,
                contentType, safetyLevel, hiddenFromSearch,
                x => result.NotifyTaskCompletionSource(x)
                );
            return result.Task;
        }


        public Task<NoResponse> PhotosetsAddPhoto(string photosetId, string photoId)
        {
            var result = new TaskCompletionSource<NoResponse>();
            _flickrService.PhotosetsAddPhotoAsync(photosetId, photoId, x => result.NotifyTaskCompletionSource(x));
            return result.Task;
        }
    }

    static class TaskExtensions
    {
        public static void NotifyTaskCompletionSource<T>(this TaskCompletionSource<T> taskCompletionSource,
            FlickrResult<T> flickrResult)
        {
            if (flickrResult.HasError)
            {
                taskCompletionSource.SetException(flickrResult.Error);
            }
            else
            {
                taskCompletionSource.SetResult(flickrResult.Result);
            }
        }
    }
}