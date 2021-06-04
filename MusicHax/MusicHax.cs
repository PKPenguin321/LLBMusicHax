using System;
using System.IO;
using UnityEngine;
using LLHandlers;
using LLScreen;

namespace MusicHax
{
    public class MusicHax : MonoBehaviour
    {
        #region modinfos
        public const string modVersion = "v2.1";
        public const string repositoryOwner = "PKPenguin321";
        public const string repositoryName = "LLBMusicHax";
        #endregion

        public static MusicHax instance = null;
        private static ModMenuIntegration MMI = null;
        private static readonly string MHResourcesPath = Path.Combine(Path.Combine(Application.dataPath, "Managed"), "MusicHaxResources");

        private static AudioCache musicCache = new AudioCache();
        private static AudioCache sfxCache = new AudioCache();
        private static AudioCache voiceCache = new AudioCache();


        public static void Initialize()
        {
            GameObject gameObject = new GameObject("MusicHax");
            instance = gameObject.AddComponent<MusicHax>();
            DontDestroyOnLoad(gameObject);
        }

        #region configs
        private static bool PreloadingEnabled
        {
            get
            {
                /*if (MMI != null)
                    return MMI.GetTrueFalse("(bool)enablePreloading");*/
                return true;
            }
        }
        #endregion

        private bool loadingExternalMusicFiles = false;
        private bool didLoad = false;
        private const string loadingText = "MusicHax is loading External Sounds...";

        void Update()
        {
            if (false && MMI == null) { MMI = gameObject.AddComponent<ModMenuIntegration>(); }
            else
            {
                if (PreloadingEnabled)
                {
                    if (this.didLoad == false)
                    {
                        loadingExternalMusicFiles = true;
                        didLoad = true;
                        LoadMusics();
                        LoadSfx();
                        LoadVoice();
                    } else if (!musicCache.IsLoading && !sfxCache.IsLoading && !voiceCache.IsLoading)
                    {
                        UIScreen.SetLoadingScreen(false);
                        loadingExternalMusicFiles = false;
                    }
                }
            }

            if (DNPFJHMAIBP.AKGAOAEJ != null && DNPFJHMAIBP.AKGAOAEJ.musicAssetLinks != null)
            {
                CreateMusicDirectories();
            }
        }

        private void OnGUI()
        {
            var OriginalColor = GUI.contentColor;
            var OriginalLabelFontSize = GUI.skin.label.fontSize;
            var OriginalLabelAlignment = GUI.skin.label.alignment;

            GUI.contentColor = Color.white;
            GUI.skin.label.fontSize = 50;
            if (UIScreen.loadingScreenActive && this.loadingExternalMusicFiles == true)
            {
                GUIStyle label = new GUIStyle(GUI.skin.label);
                var sX = Screen.width / 2;
                var sY = UIScreen.GetResolutionFromConfig().height / 3;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0, sY-25, Screen.width, sY-75), loadingText);
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }
            GUI.contentColor = OriginalColor;
            GUI.skin.label.fontSize = OriginalLabelFontSize;
            GUI.skin.label.alignment = OriginalLabelAlignment;

        }

        private void LoadMusics()
        {
            UIScreen.SetLoadingScreen(true, false, false, Stage.NONE);

            musicCache.Clear();

            DirectoryInfo[] musicDirectories = Directory.CreateDirectory(Path.Combine(MHResourcesPath, "Music")).GetDirectories();
            foreach (DirectoryInfo musicDirectory in musicDirectories)
            {
                foreach (AudioInfo musicInfo in AudioUtils.GetAudioInfos(musicDirectory))
                {
                    Debug.Log("MusicHax: Loading new " + musicDirectory.Name + " : " + musicInfo.file.FullName);
                    musicCache.LoadClip(musicDirectory.Name, musicInfo, Path.GetFileNameWithoutExtension(musicInfo.file.FullName));
                }
            }
        }

        private void LoadSfx()
        {
            sfxCache.Clear();

            DirectoryInfo[] sfxDirectories = Directory.CreateDirectory(Path.Combine(MHResourcesPath, "Sfx")).GetDirectories();
            foreach (DirectoryInfo sfxDirectory in sfxDirectories)
            {
                foreach (AudioInfo sfxInfo in AudioUtils.GetAudioInfos(sfxDirectory))
                {
                    Debug.Log("Loading new " + sfxDirectory.Name + " : " + sfxInfo.file.FullName);
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

            DirectoryInfo[] charDirectories = Directory.CreateDirectory(Path.Combine(MHResourcesPath, "Voice")).GetDirectories();
            foreach (DirectoryInfo charDirectory in charDirectories)
            {
                foreach (DirectoryInfo voiceDirectory in charDirectory.GetDirectories())
                {
                    foreach (AudioInfo voiceInfo in AudioUtils.GetAudioInfos(voiceDirectory))
                    {
                        Debug.Log("Loading new " + voiceDirectory.Name + " : " + voiceInfo.file.FullName);

                        string cacheKey = GetVoiceCacheKey(voiceDirectory.Name, GetCharacterByName(charDirectory.Name));
                        voiceCache.LoadClip(cacheKey, voiceInfo);
                    }
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
                Debug.Log("Creating directory: " + directoryname);
                Directory.CreateDirectory(Path.Combine(MHResourcesPath, directoryname));
            }
            directoryAlreadyCreated = true;
        }

        private static AudioClip GetClipNow(string musicFilePath)
        {
            return AudioUtils.GetClipSynchronously(musicFilePath);
        }

        public static AudioClip GetMusicClip(string clipName)
        {
            Debug.Log("MusicHax: Got asked for a clip named: \"" + clipName + "\"");
            if (PreloadingEnabled && musicCache.ContainsKey(clipName) && musicCache[clipName].Count > 0)
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
                } catch (Exception e)
                {
                    Debug.LogError("MusicHax: Exception caught: ");
                    Debug.LogException(e);
                }
                return null;
            }
        }

        public static AudioClip[] GetSfxClips(string audioFile)
        {
            Debug.Log("MusicHax: Got asked for a sfx clip named: \"" + audioFile + "\"");

            Directory.CreateDirectory(Path.Combine(Path.Combine(MHResourcesPath, "Sfx"), audioFile));

            if (sfxCache.ContainsKey(audioFile) && sfxCache[audioFile].Count > 0)
            {
                return sfxCache[audioFile].ToArray();
            }
            return new AudioClip[0];
        }

        public static AudioClip[] GetVoiceClips(string audioFile, Character character)
        {
            Debug.Log("MusicHax: Got asked for a voice clip named: \"" + audioFile + "\" for " + character.ToString());

            Directory.CreateDirectory(Path.Combine(Path.Combine(Path.Combine(MHResourcesPath, "Voice"), character.ToString()), audioFile));

            string cacheKey = GetVoiceCacheKey(audioFile, character);
            if (voiceCache.ContainsKey(cacheKey) && voiceCache[cacheKey].Count > 0)
            {
                return voiceCache[cacheKey].ToArray();
            }
            return new AudioClip[0];
        }

        public static Character GetCharacterByName(string characterName)
        {
            for (int i = 0; i < (int)Character._MAX_NORMAL; i++)
            {
                Character currentCharacter = (Character)i;
                if (currentCharacter.ToString() == characterName)
                {
                    return currentCharacter;
                }
            }
            return Character.NONE;
        }
    }
}
