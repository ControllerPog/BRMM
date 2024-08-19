using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.Net.Http;
using System.IO.Compression;
using static BrmmSlim.ModPack_Manager;
using System.Diagnostics;

namespace BrmmSlim
{
    internal class ModPack_Manager
    {
        public class Mod
        {
            public string Name { get; set; }
            public string Imge { get; set; }
            public string Author { get; set; }
            public string DownloadUrl { get; set; }
            public string Version { get; set; }
        }

        public class ModPack
        {
            public string IconUrl { get; set; }
            public string Name { get; set; }
            public string Author { get; set; }

            public List<Mod> Mods { get; set; }
        }
        public static void CreateModPackJson( string name, string iconUrl, string displayName, List<Mod> mods, string author)
        {
            var modPack = new ModPack
            {
                IconUrl = iconUrl,
                Name = displayName,
                Author = author,
                Mods = mods
            };

            string json = JsonConvert.SerializeObject(modPack, Formatting.Indented);
            string fileName = $"./ModPacks/{name}_modpack.json";

            File.WriteAllText(fileName, json);

            //Console.WriteLine($"Mod pack JSON saved to {fileName}");
        }

        public static void AddModToModPack(string modPackName, string Token, Mod newMod)
        {
            string fileName = $"./ModPacks/{modPackName}_modpack.json";


            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                var modPack = JsonConvert.DeserializeObject<ModPack>(json);

                bool modExists = modPack.Mods.Any(mod => mod.Name == newMod.Name);

                if (!modExists)
                {
                    modPack.Mods.Add(newMod);

                    json = JsonConvert.SerializeObject(modPack, Formatting.Indented);
                    File.WriteAllText(fileName, json);

                    //MessageBox.Show($"Mod '{newMod.Name}' added to mod pack '{modPackName}'");
                }
                else
                {
                    //MessageBox.Show($"Mod '{newMod.Name}' already exists in mod pack '{modPackName}'.");
                }
            }
            else
            {
                //MessageBox.Show($"Mod pack '{modPackName}' does not exist.");
            }
        }

        public static void RemoveModFromModPack(string modPackName, Mod modToRemove)
        {
            string fileName = $"./ModPacks/{modPackName}_modpack.json";

            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                var modPack = JsonConvert.DeserializeObject<ModPack>(json);

                var mod = modPack.Mods.FirstOrDefault(m => m.Name == modToRemove.Name);

                if (mod != null)
                {
                    modPack.Mods.Remove(mod);

                    json = JsonConvert.SerializeObject(modPack, Formatting.Indented);
                    File.WriteAllText(fileName, json);

                    //MessageBox.Show($"Mod '{modToRemove.Name}' removed from mod pack '{modPackName}'");
                }
                else
                {
                    //MessageBox.Show($"Mod '{modToRemove.Name}' does not exist in mod pack '{modPackName}'.");
                }
            }
            else
            {
                //MessageBox.Show($"Mod pack '{modPackName}' does not exist.");
            }
        }

        public static void DeleteModPack(string modPackName)
        {
            string fileName = $"./ModPacks/{modPackName}_modpack.json";

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                MessageBox.Show($"Successfully removed {modPackName} modpack.");
            }
            else
            {
                //MessageBox.Show($"Modpack '{modPackName}' does not exist.");
            }
        }

        private bool isWorking = false;
        private bool isupdates = false;

        public async void PlayModPack(string modPackName, string token, string BrickRigsPath, string SteamPath)
        {
            if (isWorking)
                return;

            isWorking = true;

            string fileName = $"./ModPacks/{modPackName}_modpack.json";

            if (!File.Exists(fileName))
            {
                Console.WriteLine("Modpack file not found.");
                isWorking = false;
                return;
            }

            string json = File.ReadAllText(fileName);
            var modPack = JsonConvert.DeserializeObject<ModPack>(json);

            string url = "https://service.brmm.ovh/api/get";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    JArray jsonArray = JArray.Parse(responseBody);

                    bool isUpdated = false;
                    List<Task> downloadTasks = new List<Task>();

                    foreach (var mods in modPack.Mods)
                    {


                        var serverMod = jsonArray.FirstOrDefault(item => item["Name"].ToString() == mods.Name);

                        if (serverMod != null)
                        {


                            string serverVersion = serverMod["Version_Mod"].ToString();
                            string serverImge = serverMod["Logo_Imge_Link"].ToString();

                            if (!File.Exists("./Mods/" + mods.Name.Replace(" ", "_") + ".zip"))
                            {

                                Console.WriteLine(File.Exists("./Mods/" + mods.Name.Replace(" ", "_") + ".zip"));

                                isupdates = true;
                                var fileDownloader = new ModStatus();
                                fileDownloader.Show();
                                downloadTasks.Add(Task.Run(() => fileDownloader.DownloadFile(token, mods.Name.Replace(" ", "_"))));
                            }

                            if (string.Compare(mods.Version, serverVersion) < 0)
                            {
                                isupdates = true;
                                // Update mod information
                                mods.Version = serverVersion;
                                mods.Imge = serverImge;

                                string modFilePath = "./Mods/" + mods.Name.Replace(" ", "_") + ".zip";


                                var fileDownloader = new ModStatus();
                                fileDownloader.Show();
                                downloadTasks.Add(Task.Run(() => fileDownloader.DownloadFile(token, mods.Name.Replace(" ", "_"))));

                                isUpdated = true;

                                Console.WriteLine($"Updated mod: {mods.Name} to version {mods.Version}");
                            }
                        }
                    }

                    await Task.WhenAll(downloadTasks);

                    Console.WriteLine("All downloads are complete.");

                    if (isUpdated)
                    {
                        string updatedJson = JsonConvert.SerializeObject(modPack, Formatting.Indented);
                        File.WriteAllText(fileName, updatedJson);
                        Console.WriteLine("Modpack updated and saved.");
                    }
                    else
                    {
                        Console.WriteLine("All mods are up-to-date.");
                    }

                    if (!isupdates)
                    {
                        if (!IsDirectoryEmpty(BrickRigsPath + "/Mods"))
                        {
                            Directory.Delete(BrickRigsPath + "/Mods", true);

                        }

                        if (!Directory.Exists(BrickRigsPath + "/Mods"))
                        {
                            Directory.CreateDirectory(BrickRigsPath + "/Mods");
                        }




                        foreach (var mods in modPack.Mods)
                        {
                            //Console.WriteLine("./Mods/" + mods.DownloadUrl.Replace(" ", "_") + " Niggers " + BrickRigsPath + "/Mods/");
                            //ZipFile.ExtractToDirectory("./Mods/" + mods.DownloadUrl.Replace(" ", "_"), BrickRigsPath + "/Mods/");

                            string modsPath = "./Mods/";
                            string zipFileName = mods.DownloadUrl.Replace(" ", "_");
                            string zipFilePath = Path.Combine(modsPath, zipFileName);
                            string extractPath = Path.Combine(BrickRigsPath, "Mods");

                            try
                            {
                                if (!File.Exists(zipFilePath))
                                {
                                    Console.WriteLine("ZIP file not found: " + zipFilePath);
                                    return;
                                }

                                bool isValidZip = true;
                                try
                                {
                                    using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                                    {
                                        if (archive.Entries.Count == 0)
                                        {
                                            isValidZip = false;
                                        }
                                    }
                                }
                                catch (InvalidDataException)
                                {
                                    isValidZip = false;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Error while checking the ZIP file: " + ex.Message);
                                    return;
                                }

                                if (!isValidZip)
                                {
                                    Console.WriteLine("The ZIP file is corrupted or empty. Deleting file: " + zipFilePath);
                                    File.Delete(zipFilePath);
                                    return;
                                }

                                if (!Directory.Exists(extractPath))
                                {
                                    Directory.CreateDirectory(extractPath);
                                }

                                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
                                Console.WriteLine("File successfully extracted.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("An error occurred during file extraction: " + ex.Message);
                            }
                        }
                    }

                    if (!isupdates)
                    {
                        string brickRigsAppID = "552100";

                        string arguments = $"-applaunch {brickRigsAppID}";

                        try
                        {
                            Process.Start(SteamPath + "/Steam.exe", arguments);
                            Console.WriteLine("Brick Rigs is starting via Steam...");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("An error occurred while trying to start Brick Rigs: " + ex.Message);
                        }
                    }

                    isUpdated = false;
                    isupdates = false;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
            }

            isWorking = false;
        }


        static bool IsDirectoryEmpty(string path)
        {
            return Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0;
        }

        public JObject GetAllModPacks()
        {
            string modPacksDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModPacks");

            var json = new JObject();

            if (Directory.Exists(modPacksDirectory))
            {
                var modPacks = Directory.GetFiles(modPacksDirectory, "*.json")
                                        .Select(File.ReadAllText)
                                        .Select(JObject.Parse)
                                        .ToList();

                json = new JObject
                {
                    ["action"] = "ReceiveModPacks",
                    ["modPacks"] = new JArray(modPacks)
                };
            }
            else
            {
                MessageBox.Show("ModPacks directory does not exist.");
            }

            return json;
        } 
    }
}
