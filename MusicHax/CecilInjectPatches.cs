using UnityEngine;
using LLHandlers;

namespace MusicHax
{
    public static class CecilInjectPatches
    {
        public static bool PlayHaxedMusic(ref AudioClip clip, ref Vector2 pLoopData )
        {
            MusicHax.CreateMusicDirectory(clip.name);
            AudioClip newClip = MusicHax.GetAudioClipFor(clip.name);
            if (newClip != null)
            {
                clip = newClip;
            }
            return true;
        }

    }
}
