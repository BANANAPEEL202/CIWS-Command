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

                // Create a rotation for the missile with a horizontal facing direction
                Quaternion horizontalRotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

                // Instantiate the missile at the random position
                GameObject missile = Instantiate(MissilePrefab, spawnPosition, horizontalRotation);

                // Set the missile's velocity
                missile.GetComponent<Rigidbody>().linearVelocity = transform.forward * missileSpeed;
            }
        }
    }
}
