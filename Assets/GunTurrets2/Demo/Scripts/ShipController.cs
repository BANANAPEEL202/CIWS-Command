using UnityEngine;

public class ShipController : MonoBehaviour
{
    // Public fields for configuration
    public float maxForwardSpeed = 10f; // Max speed the ship can reach
    public float accelerationRate = 2f; // How quickly the ship accelerates
    public float decelerationRate = 5f; // How quickly the ship decelerates
    public float maxTurnSpeed = 50f;
    public float bankAngle = 30f; // Maximum banking angle
    public float bankSpeed = 2f; // Speed of banking adjustment
    public float drag = 0.1f; // Simulates air/water resistance
    public float rockingStrength = 1f; // Strength of the rocking effect
    public float rockingSpeed = 1f; // Speed of the rocking effect

    // Internal variables
    private float currentForwardSpeed = 0f; // Tracks current speed
    private float currentBank = 0f;
    private Rigidbody rb;

    // Rocking effect variables
    private Vector3 eulers = Vector3.zero;
    private float offset = 0f;

    void Start()
    {
        // Get or add a Rigidbody to the ship
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure Rigidbody for ship movement
        rb.useGravity = false;
        rb.linearDamping = drag;
        rb.angularDamping = drag;

        // Initialize the offset for the rocking effect
        offset = Random.Range(0f, 1000f);
    }

    void Update()
    {
        // Handle input and adjust orientation
        HandleInput();
        ApplyRockingEffect();
    }

    private void HandleInput()
    {
        // Forward movement
        if (Input.GetKey(KeyCode.UpArrow))
        {
            // Gradually increase forward speed using an exponential-like curve
            currentForwardSpeed += accelerationRate * (1 - currentForwardSpeed / maxForwardSpeed) * Time.deltaTime;
        }
        else
        {
            // Gradually slow down when not accelerating
            currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, 0f, Time.deltaTime * decelerationRate);
        }

        // Turning
        float turnInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            turnInput = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            turnInput = 1f;
        }

        if (turnInput != 0f)
        {
            // Scale turn speed by current velocity magnitude
            float currentSpeed = rb.linearVelocity.magnitude;
            float turnSpeed = Mathf.Lerp(0, maxTurnSpeed, currentSpeed / maxForwardSpeed);

            // Calculate the offset position behind the ship for turning
            Vector3 offsetPosition = transform.position - transform.forward * 2f; // Offset behind the ship

            // Calculate the force direction in the horizontal plane only
            Vector3 turnForce = -1 * transform.right * turnInput * turnSpeed;
            turnForce.y = 0;

            // Project offset position onto the horizontal plane to avoid vertical rotation
            offsetPosition.y = transform.position.y;

            // Apply the turning force at the offset position
            rb.AddForceAtPosition(turnForce, offsetPosition, ForceMode.Force);
            rb.AddForce(transform.forward * (currentForwardSpeed * 0.8f), ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(transform.forward * currentForwardSpeed, ForceMode.Acceleration);
        }

        // Adjust banking angle
        float targetBank = turnInput * -bankAngle; // Negative to bank into the turn
        currentBank = Mathf.Lerp(currentBank, targetBank, Time.deltaTime * bankSpeed);

        // Apply banking visually (rotation around local z-axis)
        Vector3 localEulerAngles = transform.localEulerAngles;
        localEulerAngles.z = currentBank;
        transform.localEulerAngles = localEulerAngles;
    }

    private void FixedUpdate()
    {
        // Clamp velocity to prevent excessive speed
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxForwardSpeed);
    }

    private void ApplyRockingEffect()
    {
        // Apply the rocking effect to the ship's rotation
        var time = Time.time;
        eulers.x = Mathf.Sin(time * rockingSpeed + offset) * rockingStrength;
        eulers.z = Mathf.Cos(time * rockingSpeed * 0.8f + 11f + offset) * rockingStrength;

        transform.localEulerAngles = eulers;
    }
}
