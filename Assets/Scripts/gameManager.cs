using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class gameManager : MonoBehaviour
{
    public static gameManager instance; // Singleton for reference in other methods

    [Header("----- Player Information -----")]
    public GameObject player; // Player entity tracker
    public playerController playerScript; // Player script tracker

    [Header("----- UI -----")]
    public GameObject playerDmgFlash; //UI element when player takes damage
    public GameObject pauseMenu; //UI element for the pause screen
    public GameObject playerDeadMenu; //UI element for the death screen
    public GameObject playerWinMenu; //UI element for the win screen
    public TextMeshProUGUI enemyCounter; //UI element to count the enemies remaining in the level

    public int enemiesToKill; //The required enemy kills to win the level

    public GameObject spawnPos; //The spawn location of the level

    public bool isPaused; //Tracker to see if the game is currently paused

    // Start is called before the first frame update
    void Awake()
    {
        instance = this; // Binds the singleton on startup
        player = GameObject.FindGameObjectWithTag("Player"); // Find the Player & bind
        playerScript = player.GetComponent<playerController>(); // Find the playerController & bind
        spawnPos = GameObject.FindGameObjectWithTag("Spawn Pos"); // Find the spawn location of the level & bind
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel") && !playerDeadMenu.activeSelf && !playerWinMenu.activeSelf)
        {
            isPaused = !isPaused;
            pauseMenu.SetActive(isPaused);
            if (isPaused)
                pause();
            else
                unpause();
        }
    }

    public void pause()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void unpause()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public IEnumerator playerDamageFlash()
    {
        playerDmgFlash.SetActive(true);
        yield return new WaitForSeconds(.1f);
        playerDmgFlash.SetActive(false);
    }

    public void youWin()
    {
        playerWinMenu.SetActive(true);
        pause();
    }

    public void updateEnemyNumber()
    {
        enemiesToKill--; //Update the counter
        updateUI(); //Update the UI
        if (enemiesToKill <= 0)
            youWin();
    }

    public void updateUI()
    {
        enemyCounter.text = enemiesToKill.ToString("F0"); //Update the display
    }
}
    
