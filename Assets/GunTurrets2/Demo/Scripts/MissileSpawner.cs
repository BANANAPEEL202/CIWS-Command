using UnityEngine;

public class MissileSpawner : MonoBehaviour
{
    public GameObject MissilePrefab = null;
    public float missileSpeed = 90f;

    // Range for random position variation
    public float spawnRadius = 5f;

    // Reference to the target object
    private Transform target;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            target = GameObject.FindGameObjectWithTag("Target").transform;
            if (MissilePrefab != null && target != null)
            {
                // Randomize the spawn position within a circular area around the spawner
                Vector3 randomOffset = new Vector3(
                    Random.Range(-spawnRadius, spawnRadius),
                    Random.Range(-spawnRadius, spawnRadius), 
                    Random.Range(-spawnRadius, spawnRadius)
                );

                // Apply the random offset to the spawner's position
                Vector3 spawnPosition = transform.position + randomOffset;

                // Calculate direction towards the target
                Vector3 directionToTarget = (target.position - spawnPosition).normalized;

                // Create a rotation that points the missile towards the target
                Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);

                // Instantiate the missile at the random position with the correct rotation
                GameObject missile = Instantiate(MissilePrefab, spawnPosition, rotationToTarget);

                // Set the missile's velocity
                missile.GetComponent<Rigidbody>().linearVelocity = missile.transform.forward * missileSpeed;

                // Pass the target to the missile's script
                missile.GetComponent<Missile>().target = target;
            }
        }
    }
}
