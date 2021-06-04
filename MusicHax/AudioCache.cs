using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace MusicHax
{
    public class AudioCache
    {
        private Dictionary<string, List<AudioClip>> audioCache;
        private int runningLoadingCoroutines;

        public AudioCache()
        {
            audioCache = new Dictionary<string, List<AudioClip>>();
        }

        public int Count { get { return audioCache.Count; } }
        public bool IsLoading { get { return runningLoadingCoroutines > 0; } }

        public void LoadClip(string key, string filePath, string clipName = null)
        {
            LoadClip(filePath, new FileInfo(filePath), clipName);
        }
        public void LoadClip(string key, FileInfo file, string clipName = null)
        {
            LoadClip(key, AudioUtils.GetAudioInfo(file), clipName);
        }
        public void LoadClip(string key, AudioInfo audioInfo, string clipName = null)
        {
            MusicHax.instance.StartCoroutine(CLoadClip(key, audioInfo, clipName));

        }

        public IEnumerator CLoadClip(string key, AudioInfo audioInfo, string clipName = null)
        {
            this.runningLoadingCoroutines++;
            if (clipName == null) clipName = key;

            Debug.Log("Loading clip at: " + audioInfo.file.FullName);

            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + audioInfo.file.FullName, audioInfo.type))
            {
                UnityWebRequestAsyncOperation asyncOperation = null;
                try
                {
                    asyncOperation = uwr.SendWebRequest();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception caught: ");
                    Debug.LogError(e);
                    yield break;
                }
                yield return asyncOperation;
                while (!uwr.isDone)
                    yield return null;
                AudioClip audioClip;
                try
                {
                    audioClip = DownloadHandlerAudioClip.GetContent(uwr);
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception caught: ");
                    Debug.LogError(e);
                    yield break;
                }
                if (audioClip == null)
                {
                    Debug.LogError("Audioclip " + key + " was null");
                }
                else
                {
                    audioClip.name = clipName;
                    if (!audioCache.ContainsKey(key))
                        audioCache.Add(key, new List<AudioClip>());
                    audioCache[key].Add(audioClip);
                }
            }
            this.runningLoadingCoroutines--;
        }

        public void Clear()
        {
            audioCache.Clear();
        }
        public bool ContainsKey(string key)
        {
            return audioCache.ContainsKey(key);
        }
        public void Add(string key, AudioClip clip)
        {
            if (!audioCache.ContainsKey(key))
                audioCache.Add(key, new List<AudioClip>());
            audioCache[key].Add(clip);
        }
        public List<AudioClip> this[string key]
        {
            get { return audioCache[key]; }
        }

        public List<AudioClip> GetClips(string key)
        {
            if (audioCache.ContainsKey(key))
                return audioCache[key];
            return null;
        }
        override public string ToString()
        {
            string result = "[\n";
            foreach (string clipListIndex in audioCache.Keys)
            {
                result += "key: " + clipListIndex + "\n";
                foreach (AudioClip clip in audioCache[clipListIndex])
                {
                    result += "  - " + clip.name + "\n";
                }
                result += "---------\n";
            }
            result += "]";
            return result;
        }
    }
}
