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

    private float currentHorizontalAngle = 0f;  // Horizontal angle around the ship
    private float currentVerticalAngle = -15f;  // Vertical angle up/down

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
            // Adjust the distance based on the scroll wheel input
            distance -= scrollInput * 20f;  // Adjust the factor (20f) for scroll speed sensitivity
            distance = Mathf.Clamp(distance, minDistance, maxDistance);  // Clamp the distance to a valid range
        }

        // Calculate the new camera position, taking into account the ship's rotation and offset
        Vector3 shipPositionWithOffset = ship.position + ship.rotation * offset;  // Offset relative to ship's rotation

        // Calculate the new camera position
        float x = shipPositionWithOffset.x + distance * Mathf.Sin(currentVerticalAngle * Mathf.Deg2Rad) * Mathf.Sin(currentHorizontalAngle * Mathf.Deg2Rad);
        float y = shipPositionWithOffset.y + distance * Mathf.Cos(currentVerticalAngle * Mathf.Deg2Rad);
        float z = shipPositionWithOffset.z + distance * Mathf.Sin(currentVerticalAngle * Mathf.Deg2Rad) * Mathf.Cos(currentHorizontalAngle * Mathf.Deg2Rad);

        // Set the camera's position
        transform.position = new Vector3(x, y, z);
        
        // Make the camera look at the ship, adjusted for the offset
        transform.LookAt(shipPositionWithOffset);
    }
}
