﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NQLauncher
{
    class HelperFunctions
    {
        private static readonly List<string> WHITELISTED_DLC_FOLDERS = new List<string>(new string[] 
        {
            // Default civ5 folders
            "DLC_01",
            "DLC_02",
            "DLC_03",
            "DLC_04",
            "DLC_05",
            "DLC_06",
            "DLC_07",
            "DLC_Deluxe",
            "DLC_SP_Maps",
            "DLC_SP_Maps_2",
            "DLC_SP_Maps_3",
            "Expansion",
            "Expansion2",
            "Shared",
            "Tablet",

            // EUI folder
            "UI_bc1",
        });

        public static void LaunchGameWithMod(ModModel mod)
        {
            ConfigFile configFile = ReadConfigFile();
            string dlcFolder = configFile.gameLocation + @"\Assets\DLC\";
            string[] subDirectories = Directory.GetDirectories(dlcFolder);
            foreach (string subdirectory in subDirectories)
            {
                if(!WHITELISTED_DLC_FOLDERS.Contains(subdirectory.Split('\\').Last()))
                {
                    Directory.Delete(subdirectory, true);
                }
            }
            DirectoryCopy(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\mods\" + mod.Name, dlcFolder);
            Process.Start(configFile.gameLocation + @"\Launcher.exe");
        }

        public static ConfigFile ReadConfigFile()
        {
            if (!File.Exists(@"config.txt"))
            {
                // Create a default config file with gameLocation pointing to default steam location if it doesn't exist.
                File.WriteAllText(@"config.txt", @"{""gameLocation"":""C:\\Program Files (x86)\\Steam\\steamapps\\common\\Sid Meier's Civilization V""}" + Environment.NewLine);
            }
            return JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(@"config.txt"));
        }

        public static bool IsModDownloaded(String modName)
        {
            string modsFolder = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\mods\";
            if (!Directory.Exists(modsFolder))
            {
                Directory.CreateDirectory(modsFolder);
            }
            List<string> modsPaths = Directory.GetDirectories(modsFolder).ToList();
            foreach (string modPath in modsPaths)
            {
                string m = modPath.Split('\\').Last();
                if (modName == m)
                {
                    return true;
                }
            }
            return false;
        }

        public static ModList GetModsListFromServer()
        {
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString(Globals.SERVER_URL);
                return JsonConvert.DeserializeObject<ModList>(json);
            }
        }
        
        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }
            
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath);
            }
        }
    }
}
