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
            Target target = other.GetComponent<Missile>().target.gameObject.GetComponent<Target>();
            if (target != null)
            {
                target.KillTarget();
            }
            Destroy(other.gameObject);
        }
    }
}
