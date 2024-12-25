using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    private BulletPool bulletPool;
    public GameObject explosionPrefab;

    private void Awake()
    {
        // Get a reference to the BulletPool (assuming it's on the same GameObject or a parent object)
        bulletPool = FindFirstObjectByType<BulletPool>(); 
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with a "Missile"
        if (collision.gameObject.CompareTag("Missile"))
        {
            // Destroy the missile (or return it to the pool if it has its own pool)
            Instantiate(explosionPrefab, transform.position, collision.transform.rotation);
            Destroy(collision.gameObject); // You can change this to returning to a missile pool if necessary


            // Return the bullet to the pool
            bulletPool.ReturnBullet(gameObject); // Return the bullet instead of destroying it
        }
    }
}
