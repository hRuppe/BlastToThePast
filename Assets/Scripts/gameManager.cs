using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;
using UnityEngine.SceneManagement;

public class gameManager : MonoBehaviour
{
    public static gameManager instance; // Singleton for reference in other methods

    [Header("----- Camera -----")]
    public new CinemachineFreeLook camera;

    [Header("----- Player Information -----")]
    public GameObject player; // Player entity tracker
    public playerController playerScript; // Player script tracker

    [Header("----- UI -----")]
    public GameObject playerDmgFlash; //UI element when player takes damage
    public GameObject pauseMenu; //UI element for the pause screen
    public GameObject playerDeadMenu; //UI element for the death screen
    public GameObject playerWinMenu; //UI element for the win screen
    public GameObject controlsMenu; // UI element for controls
    public TextMeshProUGUI enemyCounter; //UI element to count the enemies remaining in the level
    public Image healthBar;
    public Image soundBar;
    public Image itemIcon; 

    public int enemiesToKill; //The required enemy kills to win the level
    public bossAI bossToWin;

    public GameObject spawnPos; //The spawn location of the level

    public bool isPaused; //Tracker to see if the game is currently paused

    public bool hasItem;

    public GameObject[] exitSpawners;

    void Awake()
    {
        // Lock & hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        instance = this; // Binds the singleton on startup
        player = GameObject.FindGameObjectWithTag("Player"); // Find the Player & bind
        playerScript = player.GetComponent<playerController>(); // Find the playerController & bind
        spawnPos = GameObject.FindGameObjectWithTag("Spawn Pos"); // Find the spawn location of the level & bind
    }

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

        if (hasItem && !itemIcon.IsActive())
        {
            itemIcon.gameObject.SetActive(true); 
            for (int i = 0; i < exitSpawners.Length; i++)
            {
                exitSpawners[i].SetActive(true); 
            }
        }

        if (SceneManager.GetActiveScene().name == "KeegenScene")
        {
            StartCoroutine(LoadWinScene());
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
        updateUI(-1); //Update the UI
        //if (enemiesToKill <= 0) // Commented out so win will be based on escaping with item
            //youWin();
    }

    public void updateUI(int amount)
    {
        enemiesToKill += amount;
        enemyCounter.text = enemiesToKill.ToString("F0"); //Update the display
    }

    IEnumerator LoadWinScene()
    {
        if (!bossToWin.IsAlive())
        {
            yield return new WaitForSeconds(5); 
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}
    
