using UnityEngine;

public class Missile : MonoBehaviour
{
    private BulletPool bulletPool;
    public GameObject explosionPrefab;
    public int health = 3;

    // Target tracking and flight parameters
    public Transform target;
    public float rotationSpeed = 45f; // Speed at which the missile turns towards the target
    public float missileSpeed = 90f; // Speed at which the missile moves
    public float lowAltitude = 10f; // Height for the low flight phase
    public float ascendDistanceThreshold = 200f;
    public float ascendHeight = 100f; // Height for the ascend phase
    public float proximityThreshold = 20f; // Distance to target to start the dive
    private Vector3 midpointTarget; // Store the calculated midpoint target

    private Rigidbody rb;
    private enum MissileState { LowFlight, Ascend, Dive }
    private MissileState state = MissileState.LowFlight;
    private Vector3 randomOffset;

    private void Awake()
    {
        bulletPool = FindFirstObjectByType<BulletPool>();
        if (bulletPool == null)
        {
            Debug.LogError("No BulletPool found in the scene!");
        }
        rb = GetComponent<Rigidbody>();
        randomOffset = new Vector3(
            Random.Range(-10f, 10f),
            0,
            Random.Range(-10f, 10f)
        );
    }

    private void Update()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + randomOffset;  
            Vector3 missilePosition = transform.position;

            // Determine missile behavior based on state
            switch (state)
            {
                case MissileState.LowFlight:
                    targetPosition.y = lowAltitude; // Fly low
                    if (Vector3.Distance(missilePosition, target.position) <= ascendDistanceThreshold)
                    {
                        state = MissileState.Ascend;
                        // Calculate the midpoint target when entering Ascend state
                        midpointTarget = new Vector3(
                            (missilePosition.x + target.position.x) / 2,  // Average X
                            target.position.y + ascendHeight,             // Set Y to the target's Y plus ascendHeight
                            (missilePosition.z + target.position.z) / 2   // Average Z
                        );
                        missileSpeed = missileSpeed - 20f;
                    }
                    break;

                case MissileState.Ascend:
                    targetPosition = midpointTarget; // Use pre-calculated midpoint target
                    if (Vector3.Distance(missilePosition, targetPosition) <= proximityThreshold)
                    {
                        state = MissileState.Dive;
                        missileSpeed = missileSpeed + 20f;
                    }
                    break;
                case MissileState.Dive:
                    targetPosition = target.position; // Dive directly onto the target
                    break;
            }

            // Calculate direction towards the adjusted target position
            Vector3 directionToTarget = targetPosition - missilePosition;
            directionToTarget.Normalize();

            // Smoothly rotate the missile towards the target
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Adjust the missile's velocity
            rb.linearVelocity = transform.forward * missileSpeed;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Bullet"))
        {
            bulletPool.ReturnBullet(collider.gameObject);
            if (health > 0)
            {
                health--;
                return;
            }
            else
            {
                Instantiate(explosionPrefab, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a sphere at the midpoint target location for visualization
        if (state == MissileState.Ascend && midpointTarget != Vector3.zero)
        {
            Gizmos.color = Color.red; // Set the color to red for visibility
            Gizmos.DrawSphere(midpointTarget, 1f); // Draw a sphere at the midpoint target with a radius of 1 unit
        }
    }

}
