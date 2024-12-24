using UnityEngine;

public class MissileSpawner : MonoBehaviour
{
    public GameObject MissilePrefab = null;

    public float missileSpeed = 15f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (MissilePrefab != null)
            {
                Quaternion horizontalRotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                GameObject missile = Instantiate(MissilePrefab, transform.position, horizontalRotation);
                missile.GetComponent<Rigidbody>().linearVelocity = transform.forward * missileSpeed;
            }
        }
    }
    
}
