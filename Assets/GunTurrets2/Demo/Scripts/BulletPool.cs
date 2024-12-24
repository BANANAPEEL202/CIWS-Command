using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 10; // Size of the pool
    [SerializeField] private float despawnDistance = 100f; // Max distance before despawning

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
            if (bullet.activeSelf && Vector3.Distance(bullet.transform.position, Vector3.zero) > despawnDistance)
            {
                ReturnBullet(bullet);
                activeBullets.RemoveAt(i);
            }
        }
    }

    // Get a bullet from the pool
    public GameObject GetBullet()
    {
        if (pool.Count > 0)
        {
            GameObject bullet = pool.Dequeue();
            if (bullet.activeSelf)
            {
                Debug.LogWarning("Bullet is already active! Consider increasing the pool size.");
                return null;
            }
            bullet.SetActive(true); // Activate the bullet when it's taken from the pool
            bullet.transform.rotation = Quaternion.identity;
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;  // Reset velocity to zero
                rb.angularVelocity = Vector3.zero;
            }
            activeBullets.Add(bullet); // Add the bullet to the active list
            return bullet;
        }

        // Optionally, you can instantiate a new bullet if the pool is empty
        // But this is generally not recommended as it negates the performance benefits of pooling
        Debug.LogWarning("Bullet pool is empty! Consider increasing the pool size.");
        return null;
    }

    // Return a bullet to the pool
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false); // Deactivate the bullet before returning it to the pool
        pool.Enqueue(bullet);
    }
}
