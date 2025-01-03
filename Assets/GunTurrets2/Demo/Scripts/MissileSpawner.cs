using UnityEngine;
using System.Collections;

public class MissileSpawner : MonoBehaviour
{
    public GameObject MissilePrefab = null;
    public GameObject ship;

    // Range for random position variation
    public float spawnRadius = 5f;

    // Wave configuration
    public float initialDelay = 5f;  // First wave delay in seconds
    public float waveIntervalMin = 10f;  // Minimum time between waves
    public float waveIntervalMax = 14f;  // Maximum time between waves
    public int minMissilesPerWave = 4;
    public int maxMissilesPerWave = 8;
    public float missileSpawnDelayMin = 0.3f;
    public float missileSpawnDelayMax = 1f;

    private bool isSpawning = false;
    private int missileCount = 1;

    void Start()
    {
        // Start the wave spawning coroutine
        StartCoroutine(SpawnWaves());
    }

    void Update()
    {
        // Update the spawner's position relative to the ship
        transform.position = ship.transform.position + new Vector3(1500, 0, 0);

        // Allow manual missile spawning by pressing Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnMissile();
        }
    }

    IEnumerator SpawnWaves()
    {
        // Initial delay before the first wave
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // Determine the number of missiles in the wave
            int missilesInWave = Random.Range(minMissilesPerWave, maxMissilesPerWave + 1);

            for (int i = 0; i < missilesInWave; i++)
            {
                SpawnMissile();

                // Delay before spawning the next missile in the wave
                float delay = Random.Range(missileSpawnDelayMin, missileSpawnDelayMax);
                yield return new WaitForSeconds(delay);
            }

            // Wait for the next wave
            float waveInterval = Random.Range(waveIntervalMin, waveIntervalMax);
            yield return new WaitForSeconds(waveInterval);
        }
    }

    void SpawnMissile()
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

            // Pass the target to the missile's script
            missile.GetComponent<Missile>().target = randomTarget;
            missile.GetComponent<Missile>().number = missileCount;
            missileCount++;
        }
    }
}
