using UnityEngine;

public class Sky : MonoBehaviour
{
    public float scrollSpeed = 0.2f;
    Renderer rend;
    public Transform skyDome;
    public Transform clouds;
    public Transform ship;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float offset = Time.time * scrollSpeed/200;
        rend.material.mainTextureOffset = new Vector2(offset, 0);
        skyDome.position = new Vector3(ship.position.x, skyDome.position.y, ship.position.z);
        clouds.position = new Vector3(ship.position.x, clouds.position.y, ship.position.z);
    }
    
}
