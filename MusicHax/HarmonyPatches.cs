using HarmonyLib;
using UnityEngine;
using LLHandlers;

namespace MusicHax
{
    public static class PlayMusicPatch
    {
        [HarmonyPatch(typeof(LLHandlers.AudioHandler), nameof(LLHandlers.AudioHandler.PlayMusic), typeof(AudioClip), typeof(Vector2))]
        [HarmonyPrefix]
        public static bool PlayMusic_Prefix(ref AudioClip clip)
        {
            MusicHax.CreateMusicDirectory(clip.name);
            AudioClip newClip = MusicHax.GetAudioClipFor(clip.name);
            if(newClip != null)
            {
                clip = newClip;
            }
            return true;
        }
    }
}
