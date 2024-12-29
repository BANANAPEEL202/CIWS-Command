using UnityEngine;

public class ShipMeshCollider : MonoBehaviour
{
    
    public GameObject explosionPrefab;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Missile"))
        {
            Vector3 collisionPoint = other.gameObject.transform.position;
            Instantiate(explosionPrefab, collisionPoint, Quaternion.identity);
            Destroy(other.gameObject);
        }
    }
}
