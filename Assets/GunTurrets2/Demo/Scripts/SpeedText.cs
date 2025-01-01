using UnityEngine;
using UnityEngine.UI; // Use TMPro if you're using TextMeshPro
using TMPro;  // Use TextMeshPro namespace

public class SpeedText : MonoBehaviour
{
    public TMP_Text fullText;  // Reference to the FULL text UI element
    public TMP_Text halfText;  // Reference to the 1/2 text UI element
    public TMP_Text stopText;  // Reference to the STOP text UI element
    public TMP_Text backText;  // Reference to the BACK text UI element
    public Transform Ship;
    public TMP_Text currentSpeedText;  // Reference to the current speed text UI element

    void Update()
    {
        // Call this whenever the speed state changes
        UpdateSpeedModeUI();
    }

    public float MapValue(float x)
    {
        float xMin = -5f;
        float xMax = 10f;
        float yMin = -278f;
        float yMax = -195f;

        // Apply the mapping formula
        float y = yMin + ((x - xMin) * (yMax - yMin)) / (xMax - xMin);
        return y;
    }

    private void UpdateSpeedModeUI()
    {
        // Define colors for active and inactive states
        Color activeColor = new Color(1f, 1f, 1f, 1f);  // Full opacity (white)
        Color inactiveColor = new Color(1f, 1f, 1f, 0.5f); // 50% transparent (white)

        ShipController.SpeedState currentSpeedState = Ship.GetComponent<ShipController>().currentSpeedState;
        // Set colors based on the current speed state
        fullText.color = (currentSpeedState == ShipController.SpeedState.FullForward) ? activeColor : inactiveColor;
        halfText.color = (currentSpeedState == ShipController.SpeedState.HalfForward) ? activeColor : inactiveColor;
        stopText.color = (currentSpeedState == ShipController.SpeedState.Stop) ? activeColor : inactiveColor;
        backText.color = (currentSpeedState == ShipController.SpeedState.Reverse) ? activeColor : inactiveColor;

        float currentSpeed = Ship.GetComponent<ShipController>().currentForwardSpeed;
        float transformedCurrentSpeed = currentSpeed * 3;
        currentSpeedText.text = "< " + transformedCurrentSpeed.ToString("F1") + " kts";
        currentSpeedText.rectTransform.anchoredPosition = new Vector2(currentSpeedText.rectTransform.anchoredPosition.x, MapValue(currentSpeed));
    }

}
