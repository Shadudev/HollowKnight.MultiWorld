using UnityEngine;

namespace ItemSyncMod.Extras
{
    internal class AudioPlayer
    {
        public static void PlayAudio(AudioClip audioClip)
        {
            if (AreSoundsMuted()) return;

            AudioSource.PlayClipAtPoint(audioClip, new Vector3(
                Camera.main.transform.position.x - 2,
                Camera.main.transform.position.y,
                Camera.main.transform.position.z + 2),
                GameManager.instance.gameSettings.masterVolume * GameManager.instance.gameSettings.soundVolume);
            AudioSource.PlayClipAtPoint(audioClip, new Vector3(
                Camera.main.transform.position.x + 2,
                Camera.main.transform.position.y,
                Camera.main.transform.position.z + 2),
                GameManager.instance.gameSettings.masterVolume * GameManager.instance.gameSettings.soundVolume);
        }

        private static bool AreSoundsMuted()
        {
            return GameManager.instance.gameSettings.masterVolume == 0 || GameManager.instance.gameSettings.soundVolume == 0;
        }
    }
}
