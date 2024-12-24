using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Missile"))
        {
            Destroy(collision.gameObject); // Destroy the missile
            Destroy(gameObject); // Destroy the projectile
        }
    }
}
