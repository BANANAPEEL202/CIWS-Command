using System;
using Microsoft.Unity.VisualStudio.Editor;
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
        if (!gameOver && target1.isAlive == false && target2.isAlive == false && target3.isAlive == false)
        {
            gameOver = true;
            missileInterceptedText.text = "Missiles intercepted: " + (ship.GetComponent<ShipController>().missileCount-1);
            missileInterceptedText.gameObject.SetActive(true);
            gameOverImage.SetActive(true);

        }
    }
}
