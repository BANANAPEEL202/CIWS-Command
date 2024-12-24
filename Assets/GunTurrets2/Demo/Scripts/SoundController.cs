using System.Collections;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the single AudioSource
    public AudioClip ciwsStartUp;   // Spooling up sound
    public AudioClip ciwsFiring;    // Continuous firing sound
    public AudioClip ciwsAfterFire; // Spooling down sound

    private bool isFiring = false;

    void Start()
    {
        // Ensure the AudioSource is properly configured
        audioSource.loop = false; // StartUp and AfterFire are not looped
    }

   public void StartFiring()
    {
        if (!isFiring)
        {
            isFiring = true;

            double firingStartTime = AudioSettings.dspTime;
            audioSource.clip = ciwsFiring;
            audioSource.loop = true; // Firing sound is looped
            audioSource.PlayScheduled(firingStartTime);
            }
        }

    public void StopFiring()
    {
        if (isFiring)
        {
            isFiring = false;

            // Stop the firing loop and schedule the after-fire sound
            double stopTime = AudioSettings.dspTime;

            // Play the after-fire sound seamlessly
            audioSource.clip = ciwsAfterFire;
            audioSource.loop = false;
            audioSource.PlayScheduled(stopTime);
        }
    }
}
