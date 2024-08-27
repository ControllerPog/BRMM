using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BrmmSlim.ModPack_Manager;

namespace BrmmSlim
{
    public partial class ModPackStatus : Form
    {
        WebClient wc;
        ModPack mopdack_global;
        int mod = 0;
        private readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        public ModPackStatus()
        {
            InitializeComponent();
        }

        private void ModStatus_Load(object sender, EventArgs e)
        {
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string downloadProgress = e.ProgressPercentage + "%";

            double downloadSpeedInBytesPerSecond = e.BytesReceived / stopwatch.Elapsed.TotalSeconds;
            string downloadSpeed = string.Format("{0} MB/s", (e.BytesReceived / 1024.0 / 1024.0 / stopwatch.Elapsed.TotalSeconds).ToString("0.00"));

            string downloadedMBs = Math.Round(e.BytesReceived / 1024.0 / 1024.0, 2) + " MB";
            string totalMBs = Math.Round(e.TotalBytesToReceive / 1024.0 / 1024.0, 2) + " MB";

            string progress = $"{downloadedMBs}/{totalMBs} ({downloadProgress}) @ {downloadSpeed}";

            string download_mods_status = $"Downloading {mopdack_global.Mods[mod].Name} {mod}/{mopdack_global.Mods.Count}";

            progressBarDownload.Invoke((MethodInvoker)delegate
            {
                label_Doanwloading_Mod.Text = download_mods_status;
                DownloadSpeedsLable.Text = progress;
                progressBarDownload.Value = e.ProgressPercentage;
            });
        }

        internal async void DownloadFile(string token, ModPack modPack, string brickRigsPath, string steamPath, ModPack_Manager modPack_Manager, Form1 form)
        {
            string modsFolderPath = "./Mods";
            mopdack_global = modPack;
            foreach (var mod in modPack.Mods)
            {
                string serverVersion = mod.Version.ToString();
                string serverImage = mod.Imge.ToString();
                string modZipPath = Path.Combine(modsFolderPath, $"{mod.Name.Replace(" ", "_")}.zip");

                if (!File.Exists(modZipPath) || string.Compare(mod.Version, serverVersion) < 0)
                {
                    mod.Version = serverVersion;
                    mod.Imge = serverImage;

                    string apiUrl = $"https://service2.brmm.ovh/api/download/{token}/{mod.Name.Replace(" ", "_")}.zip";
                    Uri uri = new Uri(apiUrl);

                    Console.WriteLine($"Starting download for mod: {mod.Name}, version: {mod.Version}");

                    try
                    {
                        await DownloadFileAsync(uri, modZipPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to download mod: {mod.Name}, Error: {ex.Message}");
                        MessageBox.Show($"Failed to download mod: {mod.Name}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    Console.WriteLine($"Finished downloading mod: {mod.Name}, version: {mod.Version}");
                }
            }

            // Nuh uh
            //this.Close();
            modPack_Manager.PlayModPack(modPack.Name, token, brickRigsPath, steamPath, false, form);
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    this.Close();
                }));
            }
            else
            {
                this.Close();
            }

        }

        private async Task DownloadFileAsync(Uri uri, string filePath)
        {
            var tcs = new TaskCompletionSource<object>();

            using (var webClient = new WebClient())
            {
                webClient.DownloadProgressChanged += Wc_DownloadProgressChanged;

                stopwatch.Restart();

                webClient.DownloadFileCompleted += (s, e) =>
                {
                    if (e.Cancelled)
                    {
                        tcs.SetCanceled();
                    }
                    else if (e.Error != null)
                    {
                        tcs.SetException(e.Error);
                    }
                    else
                    {
                        tcs.SetResult(null);
                    }

                    mod += 1;
                    stopwatch.Stop();
                };


                webClient.DownloadFileAsync(uri, filePath);

                await tcs.Task;
            }
        }



    }
}
