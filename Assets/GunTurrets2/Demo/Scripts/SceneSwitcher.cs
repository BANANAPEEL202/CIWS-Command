using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public GameOver gameOverController;

    void Update()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "Game" && gameOverController.gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Load the "Menu" scene
                SceneManager.LoadScene("Start Screen");
            }
        }
        else if (currentScene.name == "Start Screen")
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Load the "Game" scene
                SceneManager.LoadScene("Game");
            }
        }
    }
}
