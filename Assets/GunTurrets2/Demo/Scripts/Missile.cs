using UnityEngine;

public class Missile : MonoBehaviour
{
    private BulletPool bulletPool;
    public GameObject explosionPrefab;
    public int health = 3;

    private void Awake()
    {
        // Get a reference to the BulletPool (assuming it's on the same GameObject or a parent object)
        bulletPool = FindFirstObjectByType<BulletPool>(); 
    }
    
    private void OnTriggerEnter(Collider collider)
    {
        // Check if the collision is with a "Missile"
        if (collider.gameObject.CompareTag("Bullet"))
        {
            bulletPool.ReturnBullet(collider.gameObject); // Return the bullet instead of destroying it
            // Destroy the missile (or return it to the pool if it has its own pool)
            if (health > 0)
            {
                health--;
                return;
            }
            else {
                Instantiate(explosionPrefab, transform.position, gameObject.transform.rotation);
                Destroy(gameObject); // You can change this to returning to a missile pool if necessary
            }

        }
    }
}
