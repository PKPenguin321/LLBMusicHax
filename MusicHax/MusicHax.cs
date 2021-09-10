using System;
using System.IO;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LLHandlers;
using LLBML;
using LLBML.Audio;

namespace MusicHax
{
    [BepInPlugin(PluginInfos.PLUGIN_ID, PluginInfos.PLUGIN_NAME, PluginInfos.PLUGIN_VERSION)]
    [BepInDependency(LLBML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("no.mrgentle.plugins.llb.modmenu", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LLBlaze.exe")]
    public class MusicHax : BaseUnityPlugin
    {
        //public static readonly string MHResourcesPath = Path.Combine(Path.Combine(Path.Combine(Application.dataPath, "Managed"), "MusicHaxResources");
        public static readonly string MHResourcesPath = Path.Combine(BepInEx.Paths.ManagedPath, "MusicHaxResources");
        private static AudioCache musicCache = new AudioCache();

        private static ConfigEntry<bool> enablePreloading;
        private static ConfigEntry<bool> enableVanillaMusics;

        private readonly string loadingText = "MusicHax is loading External Songs...";

        // Awake is called once when both the game libs and the plug-in are loaded
        void Awake()
        {
            Logger.LogInfo("Hello, world!");

            var harmoInstance = new Harmony(PluginInfos.PLUGIN_ID);
            Logger.LogDebug("Patching AudioHandler");
            harmoInstance.PatchAll(typeof(PlayMusicPatch));

            Directory.CreateDirectory(MHResourcesPath);

            enablePreloading = this.Config.Bind<bool>("Toggles", "EnablePreloading", false);
            enableVanillaMusics = this.Config.Bind<bool>("Toggles", "EnableVanillaMusics", false);
        }

        void Start()
        {
            LLBML.Utils.ModDependenciesUtils.RegisterToModMenu(this.Info);

            if (enablePreloading.Value)
            {
                LoadingScreen.SetLoading(this.Info, true, loadingText);
                LoadMusics();
                LoadingScreen.SetLoading(this.Info, false); //TODO doesn' t work, this is async
            }
        }


        void Update()
        {
            if (DNPFJHMAIBP.AKGAOAEJ != null && DNPFJHMAIBP.AKGAOAEJ.musicAssetLinks != null)
            {
                CreateMusicDirectories();
            }
        }

        private void LoadMusics()
        {
            musicCache.Clear();

            DirectoryInfo[] musicDirectories = new DirectoryInfo(MHResourcesPath).GetDirectories();
            foreach (DirectoryInfo musicDirectory in musicDirectories)
            {
                foreach (AudioInfo musicInfo in AudioUtils.GetAudioInfos(musicDirectory))
                {
                    Logger.LogDebug("Loading new " + musicDirectory.Name + " : " + musicInfo.file.FullName);
                    musicCache.LoadClip(musicDirectory.Name, musicInfo, Path.GetFileNameWithoutExtension(musicInfo.file.FullName));
                }
            }
        }

        private void ReloadMusics()
        {
            LoadMusics();
        }

        public static void CreateMusicDirectory(string clipName)
        {
            Directory.CreateDirectory(Path.Combine(MHResourcesPath, clipName));
        }

        private bool directoryAlreadyCreated = false;
        public void CreateMusicDirectories()
        {
            if (directoryAlreadyCreated) return;
            for (int i = 1; i < (int)AudioTrack.MAX; i++)
            {
                if (i == 13) continue;
                string directoryname = DNPFJHMAIBP.AKGAOAEJ.musicAssetLinks.GetMusicAsset((AudioTrack)i).audioClipName;
                this.Logger.LogInfo("Creating directory: " + directoryname);
                Directory.CreateDirectory(Path.Combine(MHResourcesPath, directoryname));
            }
            directoryAlreadyCreated = true;
        }

        public static AudioClip GetAudioClipFor(string clipName)
        {
            Debug.Log("MusicHax: Got asked for a clip named: \"" + clipName + "\"");
            if (enablePreloading.Value && musicCache.ContainsKey(clipName) && musicCache[clipName].Count > 0)
            {
                return musicCache[clipName][UnityEngine.Random.Range(0, musicCache[clipName].Count)];
            }
            else
            {
                try
                {
                    string[] pathList = Directory.GetFiles(Path.Combine(MHResourcesPath, clipName));
                    if (pathList.Length > 0)
                    {
                        return AudioUtils.GetClipSynchronously(pathList[UnityEngine.Random.Range(0, pathList.Length - 1)]);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("MusicHax: Exception caught: ");
                    Debug.LogException(e);
                }
                return null;
            }
        }
    }
}
