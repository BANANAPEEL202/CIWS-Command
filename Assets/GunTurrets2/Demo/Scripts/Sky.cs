using UnityEngine;

public class Sky : MonoBehaviour
{
    public float scrollSpeed = 0.2f;
    Renderer rend;
    public Transform skyDome;
    public Transform clouds;
    public Transform ship;
    bool gradientSky;

    void Start()
    {
        rend = GetComponent<Renderer>();
        // Swap the fogged palette dome for the banded gradient sky with its own sun.
        Material sky = Resources.Load<Material>("ToonSky");
        if (sky != null && sky.shader.isSupported)
        {
            rend.sharedMaterial = sky;
            gradientSky = true;
            foreach (Renderer body in GetComponentsInChildren<Renderer>())
                if (body != rend) body.enabled = false;
        }
    }

    void Update()
    {
        if (!gradientSky)
        {
            float offset = Time.time * scrollSpeed/200;
            rend.material.mainTextureOffset = new Vector2(offset, 0);
        }
        skyDome.position = new Vector3(ship.position.x, skyDome.position.y, ship.position.z);
        clouds.position = new Vector3(ship.position.x, clouds.position.y, ship.position.z);
    }

}
