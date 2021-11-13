using HarmonyLib;
using UnityEngine;
using LLHandlers;
using LLBML.Audio;


namespace MusicHax
{
    public static class PlayMusicPatch
    {
        [HarmonyPatch(typeof(LLHandlers.AudioHandler), nameof(LLHandlers.AudioHandler.PlayMusic), typeof(AudioClip), typeof(Vector2))]
        [HarmonyPrefix]
        public static bool PlayMusic_Prefix(ref AudioClip clip, ref Vector2 pLoopData)
        {
            MusicHax.CreateMusicDirectory(clip.name);
            AudioAsset audioAsset = MusicHax.GetAudioAssetFor(clip.name);
            if (audioAsset?.audioClip != null)
            {
                clip = audioAsset.audioClip;
                pLoopData = audioAsset.audioInfo.loopData;
            }
            return true;
        }
        /*
        public static bool SfxInfoPreload_Prefix(SfxInfo __instance, out bool __result)
        {
            if (__instance.isLoaded)
            {
                __result = false;
                return true;
            }

            AudioClip[] clips = MusicHax.GetSfxClips(__instance.audioFile);
            if (clips.Length > 0)
            {
                __instance.audioClips = clips;
                __instance.isLoaded = true;

                __result = true;
                return true;
            }
            __result = false;
            return false;
        }

        public static bool VoiceInfoPreload_Prefix(VoiceInfo __instance, out bool __result)
        {
            if (__instance.isLoaded)
            {
                __result = false;
                return true;
            }

            AudioClip[] clips = MusicHax.GetVoiceClips(__instance.audioFile, __instance.character);
            if (clips.Length > 0)
            {
                __instance.audioClips = clips;
                __instance.isLoaded = true;

                __result = true;
                return true;
            }
            __result = false;
            return false;
        }
        */
    }
}
