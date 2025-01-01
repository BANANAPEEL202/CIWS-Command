using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Mathematics;

public class Radar : MonoBehaviour
{
    public Transform radarBeam; // The radar beam transform (for its rotation)
    public Transform ship; // The ship's transform
    public Transform radarBackground; // The radar's base transform
    public Transform playerCamera;
    public float detectionRange = 1500f; // Maximum range of radar detection
    public float beamAngle = 2f; // Beam angle in degrees
    public float rotationSpeed = -400; // Rotation speed in degrees per second
    private float radarRadius;

    public GameObject radarBlipPrefab; // Blip prefab to represent objects on the radar
    public RectTransform radarBlips; // UI canvas to draw the blips on
    public Transform cameraPOV;
    public float fadeDuration = 0.5f; // Duration for the blips to fade out

    private float radarRotation = 0f; // Current rotation of the radar beam

    public Target target;


    void Update()
    {
        if (target.isAlive == false)
        {
            radarBeam.gameObject.SetActive(false);
            radarBlips.gameObject.SetActive(false);
        }
        else {
            radarBeam.gameObject.SetActive(true);
            radarBlips.gameObject.SetActive(true);
        }
        radarRadius = radarBackground.GetComponent<RectTransform>().rect.width / 2f - 10;

        // Update the radar's rotation based on the ship's orientation
        Vector3 shipForward = ship.transform.forward;
        shipForward.y = 0; // Ignore vertical rotation
        Vector3 cameraForward = playerCamera.transform.forward;
        cameraForward.y = 0; // Ignore vertical rotation
        float angle = Vector3.SignedAngle(shipForward, cameraForward, Vector3.up);
        radarBackground.rotation = Quaternion.Euler(0, 0, 0);
        radarBlips.rotation = Quaternion.Euler(0, 0, 0) ;

        cameraPOV.rotation = Quaternion.Euler(0, 0, -angle);

        // Rotate the radar beam based on the rotation speed
        radarRotation += rotationSpeed * Time.deltaTime;

        // Apply the rotation to the radar beam
        radarBeam.rotation = Quaternion.Euler(0, 0, 0 + radarRotation);

        // radar beam 0 is -90 + -30 = -120 from front of ship (angle counter clockwise)
        float radarRotationFromFrontofShip = (radarRotation - 90 - 30)%360;

        // Find all game objects with the "Missile" tag
        GameObject[] missiles = GameObject.FindGameObjectsWithTag("Missile");

        // For each missile, check if it is within the radar's beam angle
        foreach (GameObject missile in missiles)
        {
            // Get the direction to the missile from the ship
            Vector3 missileDirection = missile.transform.position - ship.position;
            missileDirection.y = 0; // Ignore vertical direction

            // Get the angle between the ship's forward direction and the missile's direction
            // use vector3.down to get counter clockwise angle
            float missileAngle = Vector3.SignedAngle(shipForward, missileDirection, Vector3.down);
            float missileDistance = missileDirection.magnitude;

            // Calculate the absolute difference between the angles
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(radarRotationFromFrontofShip, missileAngle));
            //Debug.Log("Radar Rotation: " + radarRotationFromFrontofShip + " | Missile Angle: " + missileAngle + " | Angle Difference: " + angleDifference);
            if (angleDifference <= beamAngle / 2f && missileDistance <= detectionRange)
            {
                // Calculate the position of the blip on the radar (2D)
                float radarAngle = Mathf.Deg2Rad * (radarRotationFromFrontofShip+90);
                float radarX = missileDistance/detectionRange * radarRadius * Mathf.Cos(radarAngle) + 5 * Mathf.Cos(radarAngle);
                float radarY = missileDistance/detectionRange * radarRadius * Mathf.Sin(radarAngle) + 5 * Mathf.Sin(radarAngle);

                // Create a blip on the radar UI
                GameObject blip = Instantiate(radarBlipPrefab, radarBlips);
                RectTransform blipRect = blip.GetComponent<RectTransform>();
                blipRect.anchoredPosition = new Vector2(radarX, radarY);


                // Start the fade-out coroutine
                StartCoroutine(FadeOutBlip(blip));
            }

        }
    }

    // Coroutine to fade out the blip
    IEnumerator FadeOutBlip(GameObject blip)
    {
        Image blipImage = blip.GetComponent<Image>();
        CanvasGroup canvasGroup = blip.GetComponent<CanvasGroup>();

        // If there's no CanvasGroup, add one
        if (canvasGroup == null)
        {
            canvasGroup = blip.AddComponent<CanvasGroup>();
        }

        float elapsedTime = 0f;

        // Gradually decrease the alpha value
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - Mathf.Clamp01(elapsedTime / fadeDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }

        // Once the fade is complete, destroy the blip
        Destroy(blip);
    }
}
