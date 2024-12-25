using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float lifetime = 2f; // How long the explosion object lasts
    public AudioSource audioSource;
    public AudioClip explosionSound; // Assign the sound clip in the Inspector

    private void Start()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on the explosion object. Adding one dynamically.");
        }

        // Assign the explosion sound and play it
        audioSource.clip = explosionSound;
        audioSource.Play();

        // Destroy the explosion object after the specified lifetime
        Destroy(gameObject, lifetime);
    }
}
