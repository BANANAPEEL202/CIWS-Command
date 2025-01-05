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
        if (gameOverController != null && gameOverController.gameOver)
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

        float currentTime = Time.time;
        if (currentTime >= nextSpeedChangeTime && gameOverController != null && !gameOverController.gameOver)
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                if ((int)currentSpeedState + 1 < System.Enum.GetValues(typeof(SpeedState)).Length)
                {
                    currentSpeedState = (SpeedState)(((int)currentSpeedState + 1) % System.Enum.GetValues(typeof(SpeedState)).Length);
                    nextSpeedChangeTime = currentTime + speedChangeCooldown;
                    bellAudioSource.Play();
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                if ((int)currentSpeedState - 1 >= 0)
                {
                    currentSpeedState = (SpeedState)(((int)currentSpeedState - 1 + System.Enum.GetValues(typeof(SpeedState)).Length) % System.Enum.GetValues(typeof(SpeedState)).Length);
                    nextSpeedChangeTime = currentTime + speedChangeCooldown;
                    bellAudioSource.Play();
                }
            }
        }

        switch (currentSpeedState)
        {
            case SpeedState.Reverse:
                targetForwardSpeed = -1 * maxReverseSpeed;
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

        float k = 0.1f;
        if (currentForwardSpeed > targetForwardSpeed)
        {
            k = 0.5f;
        }
        currentForwardSpeed += (targetForwardSpeed - currentForwardSpeed) * (1 - Mathf.Exp(-k * Time.deltaTime));

        currentForwardSpeed = Mathf.Clamp(currentForwardSpeed, -maxReverseSpeed, maxForwardSpeed);

        float turnInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            turnInput = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            turnInput = 1f;
        }

        if (turnInput != 0f && gameOverController != null && !gameOverController.gameOver)
        {
            float currentSpeed = rb.linearVelocity.magnitude;
            float turnSpeed = Mathf.Lerp(0, maxTurnSpeed, currentSpeed / maxForwardSpeed);

            Vector3 offsetPosition = transform.position - transform.forward * 2f;
            Vector3 turnForce = -1 * transform.right * turnInput * turnSpeed;
            turnForce.y = 0;
            offsetPosition.y = transform.position.y;

            rb.AddForceAtPosition(turnForce, offsetPosition, ForceMode.Force);
            rb.AddForce(transform.forward * (currentForwardSpeed * 0.8f), ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(transform.forward * currentForwardSpeed, ForceMode.Acceleration);
        }

        float targetBank = turnInput * -bankAngle;
        currentBank = Mathf.Lerp(currentBank, targetBank, bankSpeed * Time.deltaTime);

        Vector3 localEulerAngles = transform.localEulerAngles;
        localEulerAngles.z = currentBank;
        transform.localEulerAngles = localEulerAngles;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxForwardSpeed);
    }

    private void ApplyRockingEffect()
    {
        float rockingRoll = Mathf.Sin(Time.time * rockingSpeed + offset) * rockingStrength;
        float rockingPitch = Mathf.Cos(Time.time * rockingSpeed * 0.8f + 11f + offset) * rockingStrength;

        Vector3 currentRotation = transform.localEulerAngles;
        currentRotation.z += rockingRoll;
        currentRotation.x += rockingPitch;

        transform.localEulerAngles = currentRotation;
    }

    private void AdjustParticleSpeed()
    {
        var main1 = sternWake.main;
        var main2 = bowWake.main;

        main1.startSpeed = maxSternWakeSpeed * currentForwardSpeed / maxForwardSpeed;
        main2.startSpeed = currentForwardSpeed > 1 ? Mathf.Max(0.3f * maxBowWakeSpeed, maxBowWakeSpeed * currentForwardSpeed / maxForwardSpeed) : 0;
    }

    private void AdjustAudioVolume()
    {
        if (shipAudioSource != null)
        {
            shipAudioSource.volume = Mathf.Lerp(0.05f, maxVolume, currentForwardSpeed / maxForwardSpeed);
        }
    }
}
