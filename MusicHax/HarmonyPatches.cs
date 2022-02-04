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
