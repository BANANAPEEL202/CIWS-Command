using Unity.Mathematics;
using UnityEngine;

public class ShipFollow : MonoBehaviour
{
    public Transform ship;  // Reference to the ship's transform
    public float distance = 60f;  // The distance from the ship
    public float rotationSpeed = 8f;  // Speed at which the camera rotates around the ship
    public float verticalRotationSpeed = 8f;  // Speed of vertical camera movement (up/down)
    public float minVerticalAngle = -90f;  // Minimum vertical angle
    public float maxVerticalAngle = -10f;  // Maximum vertical angle
    public float minDistance = 10f;  // Minimum camera distance
    public float maxDistance = 200f;  // Maximum camera distance
    public Vector3 offset = new Vector3(0, 0, 5);  // Offset from the ship's center

    public float currentHorizontalAngle = 0f;  // Horizontal angle around the ship
    public float currentVerticalAngle = -80f;  // Vertical angle up/down

    // Camera Shake Variables
    private bool isShaking = false;
    private float shakeDuration = 0f;
    private float shakeIntensity = 0f;

    void Update()
    {
        // Get mouse input
        float horizontalInput = Input.GetAxis("Mouse X");
        float verticalInput = Input.GetAxis("Mouse Y");

        // Update the horizontal and vertical angles based on the mouse input
        currentHorizontalAngle += horizontalInput * rotationSpeed;
        currentVerticalAngle -= verticalInput * verticalRotationSpeed;

        // Clamp the vertical angle to avoid excessive rotation
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);

        // Get scroll wheel input to adjust the distance
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            distance -= scrollInput * 20f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // Calculate the new camera position, taking into account the ship's rotation and offset
        Vector3 shipPositionWithOffset = ship.position + ship.rotation * offset;

        float x = shipPositionWithOffset.x + distance * Mathf.Sin(currentVerticalAngle * Mathf.Deg2Rad) * Mathf.Sin(currentHorizontalAngle * Mathf.Deg2Rad);
        float y = math.max(5, shipPositionWithOffset.y + distance * Mathf.Cos(currentVerticalAngle * Mathf.Deg2Rad));
        float z = shipPositionWithOffset.z + distance * Mathf.Sin(currentVerticalAngle * Mathf.Deg2Rad) * Mathf.Cos(currentHorizontalAngle * Mathf.Deg2Rad);

        Vector3 finalPosition = new Vector3(x, y, z);

        // Apply shake effect if active
        if (isShaking)
        {
            finalPosition += UnityEngine.Random.insideUnitSphere * shakeIntensity;
        }

        // Set the camera's position
        transform.position = finalPosition;

        // Make the camera look at the ship
        transform.LookAt(shipPositionWithOffset);
    }

    public void ShakeCamera(float duration, float intensity)
    {
        shakeDuration = duration;
        shakeIntensity = intensity;
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine());
        }
    }

    private System.Collections.IEnumerator ShakeCoroutine()
    {
        isShaking = true;
        while (shakeDuration > 0)
        {
            shakeDuration -= Time.deltaTime;
            yield return null;
        }
        isShaking = false;
    }
}
