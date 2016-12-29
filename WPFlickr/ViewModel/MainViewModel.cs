using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using PhotoBackup.Logic;
using PhotoBackup.Logic.Flickr;

namespace WPFlickr.ViewModel
{
    public class Album : ObservableObject
    {
        private string _title;
        private bool _isUploaded;

        public string Title
        {
            set
            {
                _title = value;
                RaisePropertyChanged();
            }
            get { return _title; }
        }

        public bool IsUploaded
        {
            set
            {
                _isUploaded = value;
                RaisePropertyChanged();
            }
            get { return _isUploaded; }
        }
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly DiskPhotoProvider _diskPhotoProvider;
        private readonly FlickrPhotoProvider _flickrPhotoProvider;
        public ObservableCollection<Album> Albums { get; set; } = new ObservableCollection<Album>();

        private string _selectedFolder;

        public string SelectedFolder
        {
            get { return _selectedFolder; }
            set
            {
                _selectedFolder = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand SelectFolderCommand { get; private set; }
        public RelayCommand UploadCommand { get; private set; }

        public MainViewModel()
        {
            SelectFolderCommand = new RelayCommand(SelectFolder);
            UploadCommand = new RelayCommand(UploadAlbums);
        }

        private void SelectFolder()
        {
            var dialogResult = OpenDialog();
            if (dialogResult == CommonFileDialogResult.Ok)
            {
                var diskPhotoProvider = new DiskPhotoProvider(_selectedFolder);
                var photos = diskPhotoProvider.GetPhotos();
                photos.Select()

                foreach (var s in byFolder.Select(g => g.Key.Replace(_selectedFolder, "").Trim('\\')).Distinct())
                {
                    Albums.Add(new Album {Title = s});
                }
            }
            else
            {
                Albums.Clear();
            }
        }

        private CommonFileDialogResult OpenDialog()
        {
            var dialog = new CommonOpenFileDialog {IsFolderPicker = true};
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                SelectedFolder = dialog.FileNames.Single();
            }
            return result;
        }

        private async void UploadAlbums()
        {
            await Task.Run(() => new PhotoUploader().UploadAlbums(SelectedFolder, Albums));
        }
    }
}