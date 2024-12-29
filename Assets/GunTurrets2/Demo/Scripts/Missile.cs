using UnityEngine;

public class Missile : MonoBehaviour
{
    private BulletPool bulletPool;
    public GameObject explosionPrefab;
    public int health = 3;
    
    // Reference to the target the missile will track
    public Transform target;
    public float rotationSpeed = 45; // Speed at which the missile turns towards the target
    public float missileSpeed = 90f; // Speed at which the missile moves towards the target

    private Rigidbody rb;

    private void Awake()
    {
        // Get a reference to the BulletPool (assuming it's on the same GameObject or a parent object)
        bulletPool = FindFirstObjectByType<BulletPool>();

        // Initialize the Rigidbody for physics-based movement
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (target != null)
        {
            // Calculate direction towards the target
            Vector3 randomOffset = new Vector3(
                Random.Range(-50f, 50f),
                0,
                Random.Range(-50f, 50f)
            );
            Vector3 directionToTarget = target.position - transform.position + randomOffset;
            directionToTarget.Normalize();

            // Calculate the rotation step
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // Smoothly rotate the missile towards the target using Slerp
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Adjust the missile's velocity to move in the right direction
            rb.linearVelocity = transform.forward * missileSpeed;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        // Check if the collision is with a "Bullet"
        if (collider.gameObject.CompareTag("Bullet"))
        {
            bulletPool.ReturnBullet(collider.gameObject); // Return the bullet instead of destroying it
            // Destroy the missile (or return it to the pool if it has its own pool)
            if (health > 0)
            {
                health--;
                return;
            }
            else
            {
                Instantiate(explosionPrefab, transform.position, gameObject.transform.rotation);
                Destroy(gameObject); // You can change this to returning to a missile pool if necessary
            }
        }
    }
}
