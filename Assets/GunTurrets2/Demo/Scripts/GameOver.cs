using System;
using TMPro;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform ship;
    public Target target1;
    public Target target2;
    public Target target3;
    public GameObject gameOverImage;
    public TMP_Text missileInterceptedText;

    public Boolean gameOver = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // With damage control, the ship is lost once all four modules are down, even mid-repair.
        ShipModules modules = ship != null ? ship.GetComponent<ShipModules>() : null;
        bool destroyed = modules != null
            ? !modules.IsOperational(0) && !modules.IsOperational(1) && !modules.IsOperational(2) && !modules.IsOperational(3)
            : !target1.isAlive && !target2.isAlive && !target3.isAlive;
        if (!gameOver && destroyed)
        {
            gameOver = true;
            missileInterceptedText.text = "Missiles intercepted: " + (ship.GetComponent<ShipController>().missileCount-1);
            missileInterceptedText.gameObject.SetActive(true);
            gameOverImage.SetActive(true);

        }
    }
}
