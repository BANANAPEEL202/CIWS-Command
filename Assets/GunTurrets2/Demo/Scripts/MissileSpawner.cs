using UnityEngine;

public class MissileSpawner : MonoBehaviour
{
    public GameObject MissilePrefab = null;
    public float missileSpeed = 90f;

    // Range for random position variation
    public float spawnRadius = 5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (MissilePrefab != null)
            {
                // Randomize the spawn position within a circular area around the spawner
                Vector3 randomOffset = new Vector3(
                    Random.Range(-spawnRadius, spawnRadius),
                    Random.Range(-spawnRadius, spawnRadius), 
                    Random.Range(-spawnRadius, spawnRadius)
                );

                // Apply the random offset to the spawner's position
                Vector3 spawnPosition = transform.position + randomOffset;

                // Calculate direction towards the origin
                Vector3 target = new Vector3(50, 50, 50);
                Vector3 directionToOrigin = (target - spawnPosition).normalized;

                // Create a rotation that points the missile towards the origin
                Quaternion rotationToOrigin = Quaternion.LookRotation(directionToOrigin);

                // Instantiate the missile at the random position
                GameObject missile = Instantiate(MissilePrefab, spawnPosition, rotationToOrigin);

                // Set the missile's velocity
                missile.GetComponent<Rigidbody>().linearVelocity = missile.transform.forward * missileSpeed;
            }
        }
    }
}
