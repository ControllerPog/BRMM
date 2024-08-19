using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrmmSlim
{
    public partial class ModStatus : Form
    {
        WebClient wc;
        public ModStatus()
        {
            InitializeComponent();
        }

        private void ModStatus_Load(object sender, EventArgs e)
        {
            wc = new WebClient();
            wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
            wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Random rnd = new Random();
            Invoke(new MethodInvoker(delegate ()
            {
                progressBarDownload.Minimum = 00;
                progressBarDownload.Maximum = 100;
                double total = double.Parse(e.BytesReceived.ToString());
                double recive = double.Parse(e.TotalBytesToReceive.ToString());
                double DonloadBar = recive / total * 90 + 10;
                progressBarDownload.Value=int.Parse(Math.Truncate(DonloadBar).ToString());
            }));
        }

        private async void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            await Task.Delay(1000); 

            if (e.Error == null && !e.Cancelled)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    this.Close();
                }));
            }
            else
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    MessageBox.Show("Download failed or was canceled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }
        public async Task DownloadFile(string token, string fileName)
        {
            string apiUrl = $"https://service.brmm.ovh/api/download/{token}/{fileName}.zip";
            string modsFolderPath = "./Mods";

            //main_label.Text = "Downloading " + fileName;

            if (!Directory.Exists(modsFolderPath))
            {
                Directory.CreateDirectory(modsFolderPath);
            }

            string filePath = Path.Combine(modsFolderPath, fileName + ".zip");

            Thread thread = new Thread(() =>
            {
                Uri uri = new Uri(apiUrl);
                wc.DownloadFileAsync(uri, filePath);
            });
            thread.Start();
        }
    }
}
