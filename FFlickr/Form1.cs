using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlickrNet;

namespace FFlickr
{
    public partial class Form1 : Form
    {
        private string _dir;
        Flickr f = FlickrManager.GetAuthInstance();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            MessageBox.Show(config.FilePath);
            
//            requestToken = f.OAuthGetRequestToken("oob");
//
//            string url = f.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Delete);
//
//            System.Diagnostics.Process.Start(url);
//
//            var verifier = "711-800-991";
//            FlickrManager.OAuthToken = f.OAuthGetAccessToken(requestToken, verifier);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                _dir = folderBrowserDialog1.SelectedPath;
                label1.Text = _dir;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();
            var allFiles = Directory.GetFiles(_dir, "*.*", SearchOption.AllDirectories);
            var byFolder = allFiles.GroupBy(Path.GetDirectoryName).ToList();
            foreach (var group in byFolder)
            {
                UploadAlbum(@group);
            }
            stopwatch.Stop();
            Text = stopwatch.Elapsed.ToString();
        }

        private void UploadAlbum(IGrouping<string, string> @group)
        {
            var rootDir = @group.Key;
            var filesInDir = @group.ToList();
            var albumName = rootDir.Replace(_dir, "").Trim('\\');
            var primaryPhotoId = "25167874736"; //romek
            var photoSet = f.PhotosetsCreate(albumName, primaryPhotoId);

            Text = albumName;



            var photoIds = filesInDir.AsParallel().Select(file =>
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
            }).ToList();


            f.PhotosetsSetPrimaryPhoto(photoSet.PhotosetId, photoIds.First());
            f.PhotosetsRemovePhoto(photoSet.PhotosetId, primaryPhotoId);
        }
    }
}
