using UnityEngine;

public class ShipController : MonoBehaviour
{
    // Public fields for configuration
    public float maxForwardSpeed = 10f; // Max speed the ship can reach
    public float maxReverseSpeed = 5f; // Max speed in reverse
    public float maxTurnSpeed = 50f;
    public float bankAngle = 30f; // Maximum banking angle
    public float bankSpeed = 2f; // Speed of banking adjustment
    public float drag = 0.1f; // Simulates air/water resistance
    public float rockingStrength = 1f; // Strength of the rocking effect
    public float rockingSpeed = 1f; // Speed of the rocking effect
    public AudioSource bellAudioSource;
    public AudioSource shipAudioSource; // Reference to the AudioSource
    public float maxVolume = 1f;        // Maximum volume

    // Internal variables
    public float currentForwardSpeed = 0f; // Tracks current speed
    private float targetForwardSpeed = 0f; // Target speed based on state
    private float currentBank = 0f;
    private Rigidbody rb;

    // Rocking effect variables
    private float offset = 0f;

    // Particle systems for the ship
    public ParticleSystem sternWake; // First particle system
    public float maxSternWakeSpeed = 1f; // Max speed for the stern wake
    public ParticleSystem bowWake; // Second particle system
    public float maxBowWakeSpeed = 10f; // Max speed for the bow wake

    public enum SpeedState { Reverse, Stop, HalfForward, FullForward }
    public SpeedState currentSpeedState = SpeedState.Stop;

    // Internal variables
    private float speedChangeCooldown = 0.5f; // Cooldown in seconds
    private float nextSpeedChangeTime = 0f; // Tracks when the speed state can be updated next

    public int missileCount = 1;

    public GameOver gameOverController;

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

        // Ensure the AudioSource is attached
        if (shipAudioSource == null)
        {
            shipAudioSource = GetComponent<AudioSource>();
        }

        if (shipAudioSource != null)
        {
            shipAudioSource.loop = true;
            shipAudioSource.Play();
        }
    }

    void Update()
    {
        // Handle input and adjust orientation
        if (gameOverController != null && gameOverController.gameOver == true)
        {
            currentSpeedState = SpeedState.Stop;
            HandleInput();
            AdjustParticleSpeed();
            AdjustAudioVolume();
            return;
        }
        HandleInput();
        ApplyRockingEffect();
        AdjustParticleSpeed();
        AdjustAudioVolume();
    }

    private void HandleInput()
    {
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        /*
        // Reset targets (if needed)
        if (Input.GetKey(KeyCode.R)){
            GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");
            foreach (GameObject target in targets){
                Target targetScript = target.GetComponent<Target>();
                targetScript.ResetTarget();
            }
        }
        */
        float currentTime = Time.time;
        if (currentTime >= nextSpeedChangeTime && gameOverController != null && !gameOverController.gameOver)
        {
            // Forward movement
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                if ((int)currentSpeedState+1 < System.Enum.GetValues(typeof(SpeedState)).Length)
                {
                    currentSpeedState = (SpeedState)(((int)currentSpeedState + 1) % System.Enum.GetValues(typeof(SpeedState)).Length);
                    nextSpeedChangeTime = currentTime + speedChangeCooldown; // Set the next allowed time
                    bellAudioSource.Play();
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                if ((int)currentSpeedState-1 >= 0) {
                    currentSpeedState = (SpeedState)(((int)currentSpeedState - 1 + System.Enum.GetValues(typeof(SpeedState)).Length) % System.Enum.GetValues(typeof(SpeedState)).Length);
                    nextSpeedChangeTime = currentTime + speedChangeCooldown; // Set the next allowed time
                    bellAudioSource.Play();
                }
            }
            /*
            else
            {
                // Gradually slow down when not accelerating
                currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, 0f, Time.deltaTime * decelerationRate);
            }
            */
        }

        switch (currentSpeedState)
        {
            case SpeedState.Reverse:
                targetForwardSpeed = -1*maxReverseSpeed;
                break;
            case SpeedState.Stop:
                targetForwardSpeed = 0f;
                break;
            case SpeedState.HalfForward:
                targetForwardSpeed = maxForwardSpeed * 0.5f;
                break;
            case SpeedState.FullForward:
                targetForwardSpeed = maxForwardSpeed;
                break;
        }

       // Exponential growth towards the target speed
    float k = 0.1f; // Controls the steepness of the exponential curve
    if (currentForwardSpeed > targetForwardSpeed) {
        k = 0.5f;
    }
    currentForwardSpeed += (targetForwardSpeed - currentForwardSpeed) * (1 - Mathf.Exp(-k * Time.deltaTime));

        // Prevent overshooting the target speed
        currentForwardSpeed = Mathf.Clamp(currentForwardSpeed, -maxReverseSpeed, maxForwardSpeed);
        //Debug.Log(currentSpeedState + " | " + currentForwardSpeed);

        // Turning
        float turnInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            turnInput = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            turnInput = 1f;
        }

        if (turnInput != 0f && gameOverController != null &&  gameOverController.gameOver == false)
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
        localEulerAngles.z = currentBank; // Bank rotation remains here
        transform.localEulerAngles = localEulerAngles;
    }

    private void FixedUpdate()
    {
        // Clamp velocity to prevent excessive speed
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxForwardSpeed);
    }


    private void ApplyRockingEffect()
    {
        // Apply the rocking effect to the ship's roll (z-axis) without affecting the banking
        var time = Time.time;
        float rockingRoll = Mathf.Sin(time * rockingSpeed + offset) * rockingStrength;
        float rockingPitch = Mathf.Cos(time * rockingSpeed * .8f + 11f + offset) * rockingStrength;

        // Store the current rotation, and apply the rocking only to the z-axis (roll)
        Vector3 currentRotation = transform.localEulerAngles;
        currentRotation.z += rockingRoll;
        currentRotation.x += rockingRoll;

        // Apply the new rotation
        transform.localEulerAngles = currentRotation;
    }

    private void AdjustParticleSpeed()
    {
        // Get the main modules of the particle systems
        var main1 = sternWake.main;
        var main2 = bowWake.main;

        // Adjust the start speed based on the current forward speed of the ship
        main1.startSpeed = maxSternWakeSpeed * currentForwardSpeed / maxForwardSpeed;
        if (currentForwardSpeed > 1) {
            main2.startSpeed = Mathf.Max(0.3f * maxBowWakeSpeed, maxBowWakeSpeed * currentForwardSpeed / maxForwardSpeed);
        }
        else {
            main2.startSpeed = 0;
        }

        // Swap positions if moving backward
        /*
        if (currentForwardSpeed < 0)
        {
            sternWake.transform.localPosition = new Vector3(0, 0, bowWake.transform.localPosition.z);
            bowWake.transform.localPosition = new Vector3(0, 0, sternWake.transform.localPosition.z);
        }
        else
        {
            // Reset to default positions if moving forward
            sternWake.transform.localPosition = new Vector3(0, 0, -5f); // Example position for stern wake
            bowWake.transform.localPosition = new Vector3(0, 0, 5f);  // Example position for bow wake
        }
        */
    }

    private void AdjustAudioVolume()
    {
    if (shipAudioSource != null)
    {
        // Adjust the volume based on the current forward speed
        shipAudioSource.volume = Mathf.Lerp(0.05f, maxVolume, currentForwardSpeed / maxForwardSpeed);
    }
    }
}
