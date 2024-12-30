using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject ship = null;
    [SerializeField] private int poolSize = 10; // Size of the pool
    [SerializeField] private float despawnDistance = 500f; // Max distance before despawning

    private Queue<GameObject> pool;
    private List<GameObject> activeBullets; // List to track active bullets

    private void Awake()
    {
        pool = new Queue<GameObject>();
        activeBullets = new List<GameObject>();

        // Pre-fill the pool with bullet objects
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false); // Initially, all bullets are inactive
            pool.Enqueue(bullet);
        }
    }

    private void Update()
    {
        // Periodically check for bullets that are too far from the origin
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            GameObject bullet = activeBullets[i];
            if (bullet.activeSelf && Vector3.Distance(bullet.transform.position, ship.transform.position) > despawnDistance)
            {
                ReturnBullet(bullet);
            }
        }
    }

    // Get a bullet from the pool
    public GameObject GetBullet()
    {
        if (pool == null)
        {
            Debug.LogError("Bullet pool not initialized!");
            return null;
        }
        if (pool.Count > 0)
        {
            GameObject bullet = pool.Dequeue();
            if (bullet.activeSelf)
            {
                Debug.LogWarning("Bullet is already active! Consider increasing the pool size.");
                return null;
            }
            bullet.SetActive(true); // Activate the bullet when it's taken from the pool
            activeBullets.Add(bullet); // Add the bullet to the active list
            return bullet;
        }

        Debug.LogWarning("Bullet pool is empty! Consider increasing the pool size.");
        return null;
    }

    // Return a bullet to the pool
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false); // Deactivate the bullet before returning it to the pool
        activeBullets.Remove(bullet); // Remove the bullet from the active list
        bullet.transform.rotation = Quaternion.identity; // Reset rotation

        // Optional: Reset position
        bullet.transform.position = Vector3.zero; // Or set to a specific position if needed

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;  // Reset velocity to zero
            rb.angularVelocity = Vector3.zero; // Reset angular velocity
            rb.Sleep(); // Optional: Put Rigidbody to sleep
        }

        pool.Enqueue(bullet); // Return the bullet to the pool
    }
}
