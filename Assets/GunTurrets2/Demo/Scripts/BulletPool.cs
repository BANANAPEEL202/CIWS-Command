using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 10; // Size of the pool

    private Queue<GameObject> pool;

    private void Awake()
    {
        pool = new Queue<GameObject>();

        // Pre-fill the pool with bullet objects
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false); // Initially, all bullets are inactive
            pool.Enqueue(bullet);
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
