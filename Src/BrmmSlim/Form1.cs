using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BrmmSlim.ModPack_Manager;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Web.WebView2.WinForms;

namespace BrmmSlim
{
    public partial class Form1 : Form
    {
        private ModPack_Manager modPack_Manager;
        public string GamePath = "";
        public string SteamPath = "";
        public Form1()
        {
            InitializeComponent();
            modPack_Manager = new ModPack_Manager();
        }

        private void webView21_Click(object sender, EventArgs e)
        {
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            var userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AppName";
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await webView21.EnsureCoreWebView2Async(env);

            await InitializeChromium();


            string steamPath = GetSteamPath();
            if (steamPath == null)
            {
                Console.WriteLine("rip Steam.");
                return;
            }

            Console.WriteLine("directory Steam: " + steamPath);
            SteamPath = steamPath;

            string gameID = "552100";

            string gamePath = GetGameInstallPath(steamPath, gameID);

            if (gamePath != null)
            {
                Console.WriteLine("Game Path Brick Rigs: " + gamePath);
                GamePath = gamePath + "/BrickRigs";
                GamePath = GamePath.Replace(@"\", @"/");
                GamePath = GamePath.Replace(@"//", @"/");
                Console.WriteLine(GamePath);
            }
            else
            {
                Console.WriteLine("No Brick Rigs.");
            }

            string mods = $"./Mods";
            string modpacks = $"./ModPacks";

            if (!Directory.Exists(mods))
            {
                Directory.CreateDirectory(mods);
            }
            if (!Directory.Exists(modpacks))
            {
                Directory.CreateDirectory(modpacks);
            }
        }

        private async Task InitializeChromium()
        {
            await webView21.EnsureCoreWebView2Async();
            webView21.CoreWebView2.Navigate(string.Format(@"{0}\html\login.html", Application.StartupPath));
            webView21.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            webView21.CoreWebView2.WebMessageReceived += webView21_WebMessageReceived;
            webView21.ZoomFactor = 0.64;

        }

        private bool HandelMessage = false;

        private void webView21_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            Console.WriteLine(GamePath);
            if (!HandelMessage)
            {
                string message = e.TryGetWebMessageAsString();
                HandleMessageFromJavaScript(message);
                Console.WriteLine(message);

            }
        }

        public async void HandleMessageFromJavaScript(string message)
        {
            try
            {
                string script = "";
                string[] options = message.Split(',');
                //MessageBox.Show(options[0]);
                switch (options[0])
                {


                    case "CreateModPack":
                        List<Mod> mods = new List<Mod>
                        {
                            //new Mod { Name = "Mod1", DownloadUrl = "http://example.com/mod1" },
                            //new Mod { Name = "Mod2", DownloadUrl = "http://example.com/mod2" },
                        };

                        CreateModPackJson(options[1], options[2], options[1], mods, options[3]);
                        break;
                    case "GetModPacks":
                        var json1 = modPack_Manager.GetAllModPacks();
                        script = $"LoadModpacks({json1});";
                        RunAsyncScript(script);
                        break;
                    case "AddModModpack":
                        var newMod = new ModPack_Manager.Mod
                        {
                            Name = options[2],
                            Imge = options[3],
                            Author = options[4],
                            DownloadUrl = options[5],
                            Version = options[6]
                        };
                        //MessageBox.Show($"work '{options[1]}' data '{newMod}'");
                        AddModToModPack(options[1], options[7], newMod);
                        break;
                    case "DeletModFromModPack":
                        var Mod = new ModPack_Manager.Mod
                        {
                            Name = options[2],
                            Imge = options[3],
                            Author = options[4],
                            DownloadUrl = options[5],
                            Version = options[6]
                        };
                        //MessageBox.Show($"work '{options[1]}' data '{newMod}'");
                        RemoveModFromModPack(options[1], Mod);
                        break;
                    case "DeleteModpack":
                        DeleteModPack(options[1]);
                        break;
                    case "PlayModpack":
                        modPack_Manager.PlayModPack(options[1], options[2], GamePath, SteamPath, false, this);
                        break;
                    case "DownloadModpack":
                        modPack_Manager.DownloadModpack(options[1], options[2], options[3]);
                        //MessageBox.Show("works");
                        break;
                    case "Support":
                        Support();
                        break;
                    default:
                        MessageBox.Show("Unknown action from JavaScript.");
                        break;
                }
                HandelMessage = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing message from JavaScript: {ex.Message}");
            }

            HandelMessage = false;
        }

        public async void RunAsyncScript(string script)
        {
            Console.WriteLine(script);
            await webView21.CoreWebView2.ExecuteScriptAsync(script);
        }

        static string GetSteamPath()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam"))
            {
                if (key != null)
                {
                    Object o = key.GetValue("InstallPath");
                    if (o != null)
                    {
                        return o as string;
                    }
                }
            }
            return null;
        }

        static string GetGameInstallPath(string steamPath, string gameID)
        {
            string libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

            if (!File.Exists(libraryFoldersPath))
            {
                return null;
            }

            string[] libraryPaths = ParseLibraryFolders(libraryFoldersPath);

            foreach (string libraryPath in libraryPaths)
            {
                string gameManifestPath = Path.Combine(libraryPath, "steamapps", $"appmanifest_{gameID}.acf");

                if (File.Exists(gameManifestPath))
                {
                    string installDir = ParseInstallDir(gameManifestPath);
                    return Path.Combine(libraryPath, "steamapps", "common", installDir);
                }
            }

            return null;
        }

        static string[] ParseLibraryFolders(string libraryFoldersPath)
        {
            var libraryPaths = File.ReadAllLines(libraryFoldersPath)
                .Where(line => line.Contains("path"))
                .Select(line => line.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries)[3])
                .ToArray();

            return libraryPaths;
        }

        static string ParseInstallDir(string manifestPath)
        {
            var lines = File.ReadAllLines(manifestPath);

            foreach (var line in lines)
            {
                if (line.Contains("\"installdir\""))
                {
                    return line.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries)[3];
                }
            }

            return null;
        }

        private void CoreWebView2_NewWindowRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;

            Process.Start(new ProcessStartInfo(e.Uri) { UseShellExecute = true });
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            SetZoomFactor();
        }

        private void Support()
        {
            string discordInviteUrl = "https://discord.gg/JnhTBsAgqv";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = discordInviteUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void SetZoomFactor()
        {
            double initialWidth = 1300;
            double initialHeight = 700;
            double initialZoomFactor = 0.65;
            double minZoomFactor = 0.65;

            double currentWidth = this.ClientSize.Width;
            double currentHeight = this.ClientSize.Height;

            double widthFactor = currentWidth / initialWidth * 2;
            double heightFactor = currentHeight / initialHeight * 1.15;

            double newZoomFactor = initialZoomFactor * Math.Min(widthFactor,heightFactor);

            if (newZoomFactor < minZoomFactor)
            {
                newZoomFactor = minZoomFactor;
            }

            this.webView21.ZoomFactor = newZoomFactor;
        }
    }
}
