using UnityEngine;

public class MissileSpawner : MonoBehaviour
{
    public GameObject MissilePrefab = null;
    public GameObject ship;

    // Range for random position variation
    public float spawnRadius = 5f;

    void Update()
    {
        transform.position = ship.transform.position + new Vector3(2000, 0, 0);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Get all targets with the "Target" tag
            GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Target");
            
            // Filter out only alive targets
            potentialTargets = System.Array.FindAll(potentialTargets, target =>
            {
                Target targetScript = target.GetComponent<Target>();
                return targetScript != null && targetScript.isAlive; // Assuming Target has an isAlive property
            });

            // Proceed only if there are alive targets
            if (MissilePrefab != null && potentialTargets.Length > 0)
            {
                // Choose a random target from the list of alive targets
                Transform randomTarget = potentialTargets[Random.Range(0, potentialTargets.Length)].transform;

                // Randomize the spawn position within a circular area around the spawner
                Vector3 randomOffset = new Vector3(
                    Random.Range(-spawnRadius, spawnRadius),
                    0,
                    Random.Range(-spawnRadius, spawnRadius)
                );

                // Apply the random offset to the spawner's position
                Vector3 spawnPosition = transform.position + randomOffset;

                // Calculate direction towards the target
                Vector3 directionToTarget = (randomTarget.position - spawnPosition).normalized;

                // Create a rotation that points the missile towards the target
                Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);

                // Instantiate the missile at the random position with the correct rotation
                GameObject missile = Instantiate(MissilePrefab, spawnPosition, rotationToTarget);

                // Set the missile's velocity
                //missile.GetComponent<Rigidbody>().linearVelocity = missile.transform.forward * missileSpeed;

                // Pass the target to the missile's script
                missile.GetComponent<Missile>().target = randomTarget;
            }
        }
    }
}
