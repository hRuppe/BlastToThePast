using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        gameManager.instance.playerScript.Respawn();
    }

    public void nextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); 
    }

    public void newGame()
    {
        SceneManager.LoadScene("CutScene1"); 
    }

    public void OpenMainMenu()
    {
        // Stops music & destroys the music player bc the main menu will create a new one
        gameManager.instance.musicPlayer.Stop(); 
        Destroy(gameManager.instance.musicPlayer.gameObject); 
        
        SceneManager.LoadScene(0);
        resume();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void OpenControlsMenu()
    {
        gameManager.instance.controlsMenu.SetActive(true);
        gameManager.instance.pauseMenu.SetActive(false); 
    }

    public void CloseControlsMenu()
    {
        gameManager.instance.controlsMenu.SetActive(false);
        gameManager.instance.pauseMenu.SetActive(true);
    }
}
