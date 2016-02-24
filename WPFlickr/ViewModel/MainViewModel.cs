using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using WPFlickr.Service;

namespace WPFlickr.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<string> Albums { get; set; } = new ObservableCollection<string>();

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

        private void UploadAlbums()
        {
            new FlickUploader().UploadAlbums(SelectedFolder, Albums);
        }

        private void SelectFolder()
        {
            var dialogResult = OpenDialog();
            if (dialogResult == CommonFileDialogResult.Ok)
            {
                var allFiles = Directory.GetFiles(_selectedFolder, "*.*", SearchOption.AllDirectories);
                var byFolder = allFiles.GroupBy(Path.GetDirectoryName).ToList();

                foreach (var s in byFolder.Select(g => g.Key.Replace(_selectedFolder, "").Trim('\\')).Distinct())
                {
                    Albums.Add(s);
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
    }
}