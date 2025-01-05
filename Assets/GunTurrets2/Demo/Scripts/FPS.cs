using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TMP_Text fpsText;  // Assign the TMP text component in the Inspector
    private float updateInterval = 1.0f;
    private float timeAccumulator = 0.0f;
    private int frameCount = 0;

    void Update()
    {
        timeAccumulator += Time.deltaTime;
        frameCount++;

        if (timeAccumulator >= updateInterval)
        {
            float fps = frameCount / timeAccumulator;
            if (fpsText != null)
            {
                fpsText.text = $"FPS: {fps:F1}";
            }

            frameCount = 0;
            timeAccumulator = 0.0f;
        }
}
}
