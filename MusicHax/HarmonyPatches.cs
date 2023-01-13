using HarmonyLib;
using UnityEngine;
using LLHandlers;
using LLBML.Audio;


namespace MusicHax
{
    public static class PlayMusicPatch
    {
        /*[HarmonyPatch(typeof(LLHandlers.AudioHandler), nameof(LLHandlers.AudioHandler.PlayMusic), typeof(AudioClip), typeof(Vector2))]
        [HarmonyPrefix]
        public static bool PlayMusic_Prefix(ref AudioClip clip, ref Vector2 pLoopData)
        {
            MusicHax.Log.LogDebug($"Clip {clip.name} had loop data {pLoopData}");
            MusicHax.CreateMusicDirectory(clip.name);
            AudioAsset audioAsset = MusicHax.GetAudioAssetFor(clip.name);

            if (audioAsset?.audioClip == null) return true;

            clip = audioAsset.audioClip;
            pLoopData = audioAsset.audioInfo.loopData;
            return true;
        }*/

        [HarmonyPatch(typeof(LLHandlers.AudioHandler), nameof(LLHandlers.AudioHandler.PlayMusic), typeof(AudioClip), typeof(Vector2))]
        [HarmonyPrefix]
        public static bool PlayMusic_Prefix(ref AudioClip clip, ref Vector2 pLoopData)
        {
            MusicHax.Log.LogDebug($"Clip {clip.name} had loop data {pLoopData}");
            MusicHax.CreateMusicDirectory(clip.name);
            AudioAsset audioAsset = MusicHax.GetAudioAssetFor(clip.name);

            if (audioAsset?.audioClip == null) return true;

            clip = audioAsset.audioClip;
            pLoopData = audioAsset.audioInfo.loopData;
            if (DebugSettings.instance.musicOff)
            {
                return false;
            }
            if (clip == null || AudioHandler.musicPlaying == clip.name)
            {
                return false;
            }
            AudioHandler.musicLoop = pLoopData;
            if (AudioHandler.musicLoop.y <= 0f)
            {
                AudioHandler.musicLoop.y = clip.length;
            }
            AudioHandler.audioSourceMusic1.clip = clip;
            AudioHandler.audioSourceMusic2.clip = clip;
            AudioHandler.audioSourceMusic1.loop = true;
            AudioHandler.audioSourceMusic2.loop = true;
            AudioHandler.activeMusicChannel = 1;
            AudioHandler.musicPlaying = clip.name;
            if (CGLLJHHAJAK.GIGAKBJGFDI.delayMusicUntilLoaded)
            {
                AudioHandler.instance.StopKoroutine(AudioHandler.kPlayMusic, true);
                AudioHandler.instance.StartKoroutine(AudioHandler.instance.KPlayMusic(), out AudioHandler.kPlayMusic);
            }
            else if (AudioHandler.musicLoop.x > 0f)
            {
                double num = AudioSettings.dspTime + 0.01f;
                AudioHandler.nextMusicLoopTime = num + (double)(AudioHandler.musicLoop.y);
                AudioHandler.audioSourceMusic1.time = 0f;
                AudioHandler.audioSourceMusic1.PlayScheduled(num);
                AudioHandler.audioSourceMusic1.SetScheduledEndTime(AudioHandler.nextMusicLoopTime);
            }
            else
            {
                AudioHandler.audioSourceMusic1.Play();
            }
            return false;
        }



        [HarmonyPatch(typeof(LLHandlers.AudioHandler), nameof(LLHandlers.AudioHandler.Update))]
        [HarmonyPrefix]
        public static bool Update_Prefix()
        {
            if (AudioHandler.musicLoop.x > 0f && (AudioHandler.audioSourceMusic1.isPlaying || AudioHandler.audioSourceMusic2.isPlaying))
            {
                double dspTime = AudioSettings.dspTime;
                if (dspTime + 1 > AudioHandler.nextMusicLoopTime)
                {
                    if (AudioHandler.activeMusicChannel == 0)
                    {
                        AudioHandler.audioSourceMusic1.time = AudioHandler.musicLoop.x;
                        AudioHandler.audioSourceMusic1.PlayScheduled(AudioHandler.nextMusicLoopTime);
                        AudioHandler.audioSourceMusic1.SetScheduledEndTime(AudioHandler.nextMusicLoopTime + (double)AudioHandler.musicLoop.y - (double)AudioHandler.musicLoop.x);
                    }
                    else if (AudioHandler.activeMusicChannel == 1)
                    {
                        AudioHandler.audioSourceMusic2.time = AudioHandler.musicLoop.x;
                        AudioHandler.audioSourceMusic2.PlayScheduled(AudioHandler.nextMusicLoopTime);
                        AudioHandler.audioSourceMusic2.SetScheduledEndTime(AudioHandler.nextMusicLoopTime + (double)AudioHandler.musicLoop.y - (double)AudioHandler.musicLoop.x);
                    }
                    AudioHandler.activeMusicChannel = 1 - AudioHandler.activeMusicChannel;
                    AudioHandler.nextMusicLoopTime += (double)(AudioHandler.musicLoop.y - AudioHandler.musicLoop.x);
                }
            }

            return false;
        }


        [HarmonyPatch(typeof(LLHandlers.AudioHandler), nameof(LLHandlers.AudioHandler.PlayMusic), typeof(AudioClip), typeof(Vector2))]
        [HarmonyPostfix]
        public static void PlayMusic_Postfix()
        {
            MusicHax.Log.LogDebug($"At the end of PlayMusic, the loop data was {AudioHandler.musicLoop})");
        }


        [HarmonyPatch(typeof(SfxInfo), nameof(SfxInfo.Preload))]
        [HarmonyPrefix]
        public static bool SfxInfoPreload_Prefix(SfxInfo __instance, ref bool __result)
        {
            if (__instance.isLoaded)
            {
                __result = false;
                return false;
            }

            AudioClip[] clips = MusicHax.GetSfxClips(__instance.audioFile);
            if (clips != null)
            {
                __instance.audioClips = clips;
                __instance.isLoaded = true;

                __result = true;
                return false;
            }
            return true;
        }



        [HarmonyPatch(typeof(VoiceInfo), nameof(VoiceInfo.Preload))]
        [HarmonyPrefix]
        public static bool VoiceInfoPreload_Prefix(VoiceInfo __instance, ref bool __result)
        {
            if (__instance.isLoaded)
            {
                __result = false;
                return false;
            }

            AudioClip[] clips = MusicHax.GetVoiceClips(__instance.audioFile, __instance.character);
            if (clips != null)
            {
                __instance.audioClips = clips;
                __instance.isLoaded = true;

                __result = true;
                return false;
            }
            return true;
        }
    }
}
