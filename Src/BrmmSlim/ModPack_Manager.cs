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
        public class RootObject
        {
            public string Message { get; set; }
        }

        public class ModPack
        {
            public string IconUrl { get; set; }
            public string Name { get; set; }
            public string Author { get; set; }

            public string ModpackVersion { get; set; }
            public List<Mod> Mods { get; set; }
        }
        public static void CreateModPackJson( string name, string iconUrl, string displayName, List<Mod> mods, string author)
        {
            var modPack = new ModPack
            {
                IconUrl = iconUrl,
                Name = displayName,
                Author = author,
                Mods = mods,
                ModpackVersion = "1"
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

        public async void DownloadModpack(string Token, string ModpackName, string ID)
        {
            HttpClient client = new HttpClient();
            string apiUrl = $"https://service2.brmm.ovh/api/download/modpack/{Token}/{ModpackName}/{ID}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                RootObject root = JsonConvert.DeserializeObject<RootObject>(responseBody);
                string modpackJson = root.Message;

                ModPack modpackData = JsonConvert.DeserializeObject<ModPack>(modpackJson);

                // Serializuj obiekt z formatowaniem JSON
                string outputJson = JsonConvert.SerializeObject(modpackData, Formatting.Indented);

                // Usuń znaki Unicode
                outputJson = RemoveUnicodeCharacters(outputJson);

                string outputPath = $"./ModPacks/{ModpackName}_modpack.json";
                File.WriteAllText(outputPath, outputJson);

                Console.WriteLine($"Modpack '{ModpackName}' saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private string RemoveUnicodeCharacters(string input)
        {
            return new string(input.Where(c => c <= 127).ToArray());
        }


        private bool isWorking = false;
        private bool isUpdates = false;

        public async void PlayModPack(string modPackName, string token, string brickRigsPath, string steamPath, bool updateforce, Form1 form)
        {

            isUpdates = updateforce;

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

            string url = "https://service2.brmm.ovh/api/get";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    JArray jsonArray = JArray.Parse(responseBody);

                    List<Task> downloadTasks = new List<Task>();

                    // Parallel processing of mods
                    Parallel.ForEach(modPack.Mods, mod =>
                    {

                        string serverVersion = mod.Version.ToString();
                        string serverImage = mod.Imge.ToString();
                        string modZipPath = $"./Mods/{mod.Name.Replace(" ", "_")}.zip";

                        if (!File.Exists(modZipPath) || string.Compare(mod.Version, serverVersion) < 0)
                        {
                            isUpdates = true;
                            mod.Version = serverVersion;
                            mod.Imge = serverImage;
                            Console.WriteLine($"Queued download for mod: {isUpdates}, version: {mod.Version}");
                        }
                    });

                    Console.WriteLine(isUpdates);

                    if (isUpdates)
                    {
                        File.WriteAllText(fileName, JsonConvert.SerializeObject(modPack, Formatting.Indented));
                        Console.WriteLine("Modpack updated and saved.");

                        var fileDownloader = new ModPackStatus();
                        fileDownloader.Show();
                        downloadTasks.Add(Task.Run(() => fileDownloader.DownloadFile(token, modPack, brickRigsPath, steamPath, this, form)));
                    }
                    else
                    {
                        Console.WriteLine("All mods are up-to-date.");
                    }

                    // Only proceed with extraction if no new updates
                    if (!isUpdates)
                    {
                        if (Directory.Exists($"{brickRigsPath}/Mods"))
                            Directory.Delete($"{brickRigsPath}/Mods", true);

                        Directory.CreateDirectory($"{brickRigsPath}/Mods");

                        Parallel.ForEach(modPack.Mods, mod =>
                        {
                            string modZipPath = $"./Mods/{mod.Name.Replace(" ", "_")}.zip";
                            string extractPath = Path.Combine(brickRigsPath, "Mods");

                            if (File.Exists(modZipPath) && IsZipValid(modZipPath))
                            {
                                ZipFile.ExtractToDirectory(modZipPath, extractPath);
                                Console.WriteLine($"Extracted {mod.Name} to {extractPath}");
                            }
                            else
                            {
                                isUpdates = true;
                                File.Delete(modZipPath);
                                Console.WriteLine($"Invalid or missing ZIP file: {modZipPath}");
                            }
                        });
                        Console.WriteLine($"{isUpdates}");
                        LaunchBrickRigs(steamPath, form);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
            }
            finally
            {
                isWorking = false;
            }
        }

        private void LaunchBrickRigs(string steamPath, Form1 form)
        {

            try
            {
                Process.Start($"{steamPath}/Steam.exe", "-applaunch 552100");
                Console.WriteLine("Brick Rigs is starting via Steam...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while trying to start Brick Rigs: {ex.Message}");
            }

            form.HandleMessageFromJavaScript("GetModPacks");
        }

        private bool IsZipValid(string zipFilePath)
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    return archive.Entries.Count > 0;
                }
            }
            catch (InvalidDataException)
            {
                return false;
            }
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
                var modPacksFiles = Directory.GetFiles(modPacksDirectory, "*.json");
                var modPacks = new List<JObject>();

                foreach (var filePath in modPacksFiles)
                {
                    var fileContent = File.ReadAllText(filePath);
                    var modPack = JObject.Parse(fileContent);

                    if (modPack["ModpackVersion"]?.ToString() != "1")
                    {
                        modPack["ModpackVersion"] = "1";

                        File.WriteAllText(filePath, modPack.ToString());
                    }

                    modPacks.Add(modPack);
                }

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
