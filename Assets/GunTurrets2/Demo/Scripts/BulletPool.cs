using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject ship = null;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float despawnDistance = 500f;

    private Queue<GameObject> pool;
    private List<GameObject> activeBullets;

    private void Awake()
    {
        pool = new Queue<GameObject>();
        activeBullets = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            ClearTrail(bullet);
            bullet.SetActive(false);
            pool.Enqueue(bullet);
        }
    }

    private void Update()
    {
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            GameObject bullet = activeBullets[i];
            if (bullet.activeSelf && ship != null &&
                Vector3.Distance(bullet.transform.position, ship.transform.position) > despawnDistance)
                ReturnBullet(bullet);
        }
    }

    public GameObject GetBullet(Vector3 position, Quaternion rotation)
    {
        if (pool == null || pool.Count == 0) return null;
        GameObject bullet = pool.Dequeue();
        ClearTrail(bullet);

        // Teleport while inactive, before OnEnable or the trail renderer can sample a position.
        bullet.transform.SetPositionAndRotation(position, rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = position;
            rb.rotation = rotation;
        }
        activeBullets.Add(bullet);
        bullet.SetActive(true);
        TrailRenderer trail = bullet.GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.enabled = true;
            trail.Clear();
            trail.AddPosition(position);
            trail.emitting = true;
        }
        return bullet;
    }

    public void ReturnBullet(GameObject bullet)
    {
        // Multiple colliders can report the same hit; a projectile enters the queue only once.
        if (bullet == null || !activeBullets.Remove(bullet)) return;
        ClearTrail(bullet);
        bullet.SetActive(false);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }
        pool.Enqueue(bullet);
    }

    private static void ClearTrail(GameObject bullet)
    {
        TrailRenderer trail = bullet.GetComponent<TrailRenderer>();
        if (trail == null) return;
        trail.emitting = false;
        trail.Clear();
        trail.enabled = false;
    }
}
