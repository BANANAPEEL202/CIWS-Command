using UnityEngine;
using UnityEngine.Audio;

public class MasterVolumeController : MonoBehaviour
{
    public float volume = 0.1f;

    void Start()
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }
}
