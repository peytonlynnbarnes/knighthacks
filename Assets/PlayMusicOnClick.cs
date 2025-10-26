using UnityEngine;

public class PlayMusicOnClick : MonoBehaviour
{
    public AudioSource audioSource; // assign in Inspector
    public AudioClip musicClip;     // optional: choose clip in Inspector

    public void PlayMusic()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("⚠️ No AudioSource assigned!");
            return;
        }

        if (musicClip != null)
            audioSource.clip = musicClip; // change track if needed

        audioSource.Play(); // 🔊 start playing
        Debug.Log("🎵 Music started!");
    }
}
