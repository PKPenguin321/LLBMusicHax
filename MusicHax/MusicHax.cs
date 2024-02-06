using System;
using System.IO;
using System.Linq;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LLHandlers;
using LLBML;
using LLBML.Audio;
using LLBML.Utils;
using BepInEx.Logging;

namespace MusicHax
{
    [BepInPlugin(PluginInfos.PLUGIN_ID, PluginInfos.PLUGIN_NAME, PluginInfos.PLUGIN_VERSION)]
    [BepInDependency(LLBML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("no.mrgentle.plugins.llb.modmenu", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LLBlaze.exe")]
    public class MusicHax : BaseUnityPlugin
    {
        //public static readonly string MHResourcesPath = Path.Combine(Path.Combine(Path.Combine(Application.dataPath, "Managed"), "MusicHaxResources");
        //public static readonly string MHResourcesPath = Path.Combine(BepInEx.Paths.ManagedPath, "MusicHaxResources");
        internal static DirectoryInfo MHResourcesDir { get; private set; } = null;
        internal static ManualLogSource Log { get; private set; } = null;
        internal static AudioInfo currentInfo;

        private static AudioCache musicCache = new AudioCache();
        private static AudioCache sfxCache = new AudioCache();
        private static AudioCache voiceCache = new AudioCache();

        private static ConfigEntry<bool> enablePreloading;
        private static ConfigEntry<bool> enableVanillaMusics;

        private static DirectoryInfo SongsDirectory;
        private static DirectoryInfo SFXDirectory;
        private static DirectoryInfo VoicesDirectory;

        private readonly string loadingText = "MusicHax is loading External Songs...";

        // Awake is called once when both the game libs and the plug-in are loaded
        void Awake()
        {
            Log = this.Logger;
            Logger.LogInfo("Hello, world!");
            MHResourcesDir = LLBML.Utils.ModdingFolder.GetModSubFolder(this.Info);

            var harmoInstance = new Harmony(PluginInfos.PLUGIN_ID);
            Logger.LogDebug("Patching AudioHandler");
            harmoInstance.PatchAll(typeof(PlayMusicPatch));


            enablePreloading = this.Config.Bind<bool>("Toggles", "EnablePreloading", true);
            enableVanillaMusics = this.Config.Bind<bool>("Toggles", "EnableVanillaMusics", false);
            CreateDirectoryStructure();
        }

        void Start()
        {
            LLBML.Utils.ModDependenciesUtils.RegisterToModMenu(this.Info);

            if (enablePreloading.Value)
            {
                //LoadingScreen.SetLoading(this.Info, true, loadingText);
                LoadMusics();
                LoadSfx();
                LoadVoice();
                //LoadingScreen.SetLoading(this.Info, false); //TODO doesn' t work, this is async
            }
        }


        void Update()
        {
            /*
            if (DNPFJHMAIBP.AKGAOAEJ != null && DNPFJHMAIBP.AKGAOAEJ.musicAssetLinks != null)
            {
                CreateMusicDirectories();
            }
            */
        }

        private void LoadMusics()
        {
            musicCache.Clear();

            DirectoryInfo[] musicDirectories = SongsDirectory.GetDirectories();
            foreach (DirectoryInfo musicDirectory in musicDirectories)
            {
                foreach (AudioInfo musicInfo in AudioUtils.GetAudioInfos(musicDirectory))
                {
                    Logger.LogDebug("Loading new " + musicDirectory.Name + " : " + musicInfo.file.FullName);
                    musicCache.LoadClip(musicDirectory.Name, musicInfo);
                }
            }
        }

        private void LoadSfx()
        {
            sfxCache.Clear();

            DirectoryInfo[] sfxDirectories = SFXDirectory.GetDirectories();
            foreach (DirectoryInfo sfxDirectory in sfxDirectories)
            {
                foreach (AudioInfo sfxInfo in AudioUtils.GetAudioInfos(sfxDirectory))
                {
                    Logger.LogDebug("Loading new " + sfxDirectory.Name + " : " + sfxInfo.file.FullName);
                    sfxCache.LoadClip(sfxDirectory.Name, sfxInfo);
                }
            }
        }

        private static string GetVoiceCacheKey(string audioFile, Character character)
        {
            return character.ToString() + "_" + audioFile;
        }
        private void LoadVoice()
        {
            voiceCache.Clear();

            DirectoryInfo[] charDirectories = VoicesDirectory.GetDirectories();
            foreach (DirectoryInfo charDirectory in charDirectories)
            {
                foreach (DirectoryInfo voiceDirectory in charDirectory.GetDirectories())
                {
                    foreach (AudioInfo voiceInfo in AudioUtils.GetAudioInfos(voiceDirectory))
                    {
                        Logger.LogDebug("Loading new " + voiceDirectory.Name + " : " + voiceInfo.file.FullName);

                        string cacheKey = GetVoiceCacheKey(voiceDirectory.Name, CharacterApi.GetCharacterByName(charDirectory.Name));
                        voiceCache.LoadClip(cacheKey, voiceInfo);
                    }
                }
            }
        }

        private void ReloadMusics()
        {
            LoadMusics();
        }



        public static void CreateDirectoryStructure()
        {
            SongsDirectory = MHResourcesDir.CreateSubdirectory("Songs");
            foreach( string dir in FileStructure.Songs)
            {
                SongsDirectory.CreateSubdirectory(dir);
            }
            SFXDirectory = MHResourcesDir.CreateSubdirectory("SFX");
            foreach (string dir in FileStructure.SFX)
            {
                SFXDirectory.CreateSubdirectory(dir);
            }
            VoicesDirectory = MHResourcesDir.CreateSubdirectory("Voices");
            foreach(Character character in CharacterApi.GetPlayableCharacters())
            {
                var charDir = VoicesDirectory.CreateSubdirectory(character.ToString());
                foreach (string dir in FileStructure.Voices)
                {
                    charDir.CreateSubdirectory(character.ToString().ToLower() + dir);
                }
            }
        }

        public static void CreateMusicDirectory(string clipName)
        {
            Directory.CreateDirectory(Path.Combine(SongsDirectory.FullName, clipName));
        }

        private bool directoryAlreadyCreated = false;
        public void CreateMusicDirectories()
        {
            if (directoryAlreadyCreated) return;
            for (int i = 1; i < (int)AudioTrack.MAX; i++)
            {
                if (i == 13) continue; // For some reason, that number is missing from the Enum 
                string directoryname = DNPFJHMAIBP.AKGAOAEJ.musicAssetLinks.GetMusicAsset((AudioTrack)i).audioClipName;
                this.Logger.LogDebug("Creating directory: " + directoryname);
                Directory.CreateDirectory(Path.Combine(SongsDirectory.FullName, directoryname));
            }
            directoryAlreadyCreated = true;
        }

        public static AudioAsset GetAudioAssetFor(string clipName)
        {
            Log.LogDebug($"Got asked for a clip named: \"{clipName}\"");
            if (enablePreloading.Value && musicCache.ContainsKey(clipName))
            {
                int musicCount = musicCache[clipName].Count;
                Log.LogDebug($"Preloading: key exists : \"{clipName}\" | Found list length: {musicCache[clipName].Count}");
                if (enableVanillaMusics.Value)
                {
                    musicCount += 1;
                }
                if (musicCount > 0)
                {
                    int musicIndex = UnityEngine.Random.Range(0, musicCount);
                    if (enableVanillaMusics.Value)
                    {
                        musicIndex -= 1;
                    }
                    Log.LogDebug($"Rolled {musicIndex} out of {musicCount}");
                    if (musicIndex < 0) return null;
                    else return musicCache[clipName][musicIndex];
                }
            }
            else
            {
                try
                {
                    string[] pathList = Directory.GetFiles(Path.Combine(SongsDirectory.FullName, clipName));
                    if (pathList.Length > 0)
                    {
                        return AudioUtils.GetAssetSynchronously(pathList[UnityEngine.Random.Range(0, pathList.Length - 1)]);
                    }
                }
                catch (Exception e)
                {
                    Log.LogError("MusicHax: Exception caught:\n"+ e.Message);
                }
            }
            return null;
        }

        public static AudioClip GetAudioClipFor(string clipName)
        {
            return GetAudioAssetFor(clipName).audioClip;
        }

        public static AudioClip[] GetSfxClips(string audioFile)
        {
            Log.LogDebug("MusicHax: Got asked for a sfx clip named: \"" + audioFile + "\"");

            Directory.CreateDirectory(Path.Combine(SFXDirectory.FullName, audioFile));

            if (sfxCache.ContainsKey(audioFile) && sfxCache[audioFile].Count > 0)
            {
                return sfxCache[audioFile].ConvertAll<AudioClip>((AudioAsset input) => input.audioClip).ToArray();
            }
            return null;
        }

        public static AudioClip[] GetVoiceClips(string audioFile, Character character)
        {
            Log.LogDebug("MusicHax: Got asked for a voice clip named: \"" + audioFile + "\" for " + character.ToString());

            Directory.CreateDirectory(Path.Combine(Path.Combine(VoicesDirectory.FullName, character.ToString()), audioFile));

            string cacheKey = GetVoiceCacheKey(audioFile, character);
            if (voiceCache.ContainsKey(cacheKey) && voiceCache[cacheKey].Count > 0)
            {
                return voiceCache[cacheKey].ConvertAll<AudioClip>((AudioAsset input) => input.audioClip).ToArray();
            }
            return null;
        }
    }
}
