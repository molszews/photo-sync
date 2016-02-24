using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FlickrNet;

namespace FFlickr
{
    public partial class Form1 : Form
    {
        private string _dir;
        readonly Flickr fA = FlickrManager.GetInstance();
        readonly Flickr f = FlickrManager.GetAuthInstance();
        private OAuthRequestToken _requestToken;
        private string _placeholderPhotoId;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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
            _placeholderPhotoId = f.UploadPicture("pimpek.png");

            var stopwatch = Stopwatch.StartNew();
            var allFiles = Directory.GetFiles(_dir, "*.*", SearchOption.AllDirectories);
            var byFolder = allFiles.GroupBy(Path.GetDirectoryName).ToList();
            foreach (var group in byFolder)
            {
                UploadAlbum(@group);
            }
            stopwatch.Stop();
            Text = stopwatch.Elapsed.ToString();

            f.PhotosDelete(_placeholderPhotoId);
        }

        private void UploadAlbum(IGrouping<string, string> @group)
        {
            var rootDir = @group.Key;
            var filesInDir = @group.ToList();
            var albumName = rootDir.Replace(_dir, "").Trim('\\');
            var primaryPhotoId = _placeholderPhotoId;
            var photoSet = f.PhotosetsCreate(albumName, primaryPhotoId);

            Text = albumName;

            var photoIds = filesInDir.AsParallel().Select(file =>
            {
                Application.DoEvents();
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
                        Application.DoEvents();
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

        private void button3_Click(object sender, EventArgs e)
        {
            _requestToken = fA.OAuthGetRequestToken("oob");
            string url = fA.OAuthCalculateAuthorizationUrl(_requestToken.Token, AuthLevel.Delete);
            Process.Start(url);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var verifier = "711-800-991";
            verifier = textBox1.Text;
            FlickrManager.OAuthToken = fA.OAuthGetAccessToken(_requestToken, verifier);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
        }
    }
}