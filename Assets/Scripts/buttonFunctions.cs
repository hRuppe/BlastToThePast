using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public void resume()
    {
        gameManager.instance.unpause();
        gameManager.instance.pauseMenu.SetActive(false);
        gameManager.instance.isPaused = false;
    }

    public void restart()
    {
        gameManager.instance.unpause();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void quit()
    {
        Application.Quit();
    }

    public void respawn()
    {
        gameManager.instance.unpause();
        gameManager.instance.playerScript.respawn();
    }
}
