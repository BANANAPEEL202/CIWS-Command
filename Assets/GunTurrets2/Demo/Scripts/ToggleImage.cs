using UnityEngine;
using UnityEngine.UI;  // Make sure to include this if you're working with UI elements

public class ToggleImage : MonoBehaviour
{
    public Image imageToToggle;  // Reference to the image you want to show/hide
    public float toggleInterval = 1f;  // Time interval for toggling the image (default 1 second)
    private bool isVisible = true;

    void Start()
    {
        // Start the repeating action of toggling every `toggleInterval` seconds
        InvokeRepeating("ToggleVisibility", toggleInterval, toggleInterval);
    }

    void ToggleVisibility()
    {
        // Toggle the visibility
        isVisible = !isVisible;
        imageToToggle.enabled = isVisible;
    }
}
