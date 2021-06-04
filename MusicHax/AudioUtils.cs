using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MusicHax
{
    public static class AudioUtils
    {

        public static AudioClip GetClipSynchronously(string musicFilePath)
        {
            Debug.Log("MusicHax: Urgent File Load: " + musicFilePath);
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + musicFilePath, AudioType.OGGVORBIS))
            {
                try
                {
                    uwr.SendWebRequest();
                }
                catch (Exception e)
                {
                    Debug.LogError("MusicHax: Exception caught: ");
                    Debug.LogError(e);
                }
                while (!uwr.isDone)
                {
                    Thread.Sleep(20);
                }
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);

                if (audioClip == null)
                {
                    Debug.LogError("Audioclip " + musicFilePath + " was null");
                }
                else
                {
                    audioClip.name = Path.GetFileNameWithoutExtension(musicFilePath);
                    return audioClip;
                }
            }
            return null;
        }

        public static List<AudioInfo> GetAudioInfos(DirectoryInfo dir)
        {
            List<AudioInfo> audioFiles = new List<AudioInfo>();
            FileInfo[] musicFiles = dir.GetFiles();
            foreach (FileInfo file in musicFiles)
            {
                AudioInfo audioInfo = GetAudioInfo(file);
                if (audioInfo.type != AudioType.UNKNOWN)
                {
                    audioFiles.Add(audioInfo);
                }
            }
            return audioFiles;
        }

        public static AudioInfo GetAudioInfo(FileInfo file)
        {
            switch (file.Extension.ToLower())
            {
                case ".ogg":
                    return new AudioInfo(file, AudioType.OGGVORBIS);
                case ".wav":
                    return new AudioInfo(file, AudioType.WAV);
                case ".mp3":
                    return new AudioInfo(file, AudioType.MPEG);
                case ".aif":
                    return new AudioInfo(file, AudioType.AIFF);
                default:
                    Debug.LogWarning("Unsupported audio file encountered: " + file.FullName);
                    return new AudioInfo(file, AudioType.UNKNOWN);
            }
        }
    }

    public struct AudioInfo
    {
        public FileInfo file;
        public AudioType type;

        public AudioInfo(FileInfo _fi, AudioType at)
        {
            file = _fi;
            type = at;
        }
    }
}
