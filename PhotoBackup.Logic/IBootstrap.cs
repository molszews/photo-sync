namespace PhotoBackup.Logic
{
    public interface IBootstrap
    {
        IPhotoProvider RemotePhotoProvider { get; }
        IPhotoUploader Uploader { get; }
    }
}