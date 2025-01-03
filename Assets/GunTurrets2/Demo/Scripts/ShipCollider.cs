using UnityEngine;

public class ShipCollider : MonoBehaviour
{
    public GameObject explosionPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Missile"))
        {
            Vector3 collisionPoint = other.gameObject.transform.position;
            Instantiate(explosionPrefab, collisionPoint, Quaternion.identity);

            // Find the camera and trigger shake
            ShipFollow shipFollow = Camera.main.GetComponent<ShipFollow>();
            if (shipFollow != null)
            {
                shipFollow.ShakeCamera(0.5f, 1f);
            }

            Target target = other.GetComponent<Missile>().target.gameObject.GetComponent<Target>();
            if (target != null)
            {
                target.KillTarget();
            }

            other.GetComponent<Missile>().DestroyMissile();
        }
    }
}
