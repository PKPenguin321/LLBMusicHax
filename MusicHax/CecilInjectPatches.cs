using UnityEngine;
using LLHandlers;

namespace MusicHax
{
    public static class CecilInjectPatches
    {
        public static bool PlayHaxedMusic(ref AudioClip clip, ref Vector2 pLoopData)
        {
            MusicHax.CreateMusicDirectory(clip.name);
            AudioClip newClip = MusicHax.GetMusicClip(clip.name);
            if (newClip != null)
            {
                clip = newClip;
            }
            return true;
        }

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

    }
}
