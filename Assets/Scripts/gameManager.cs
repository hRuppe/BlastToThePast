using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gameManager : MonoBehaviour
{
    public static gameManager instance; // Singleton for reference in other methods

    [Header("----- Player Information -----")]
    public GameObject player; // Player entity tracker
    public playerController playerScript; // Player script tracker

    [Header("----- UI -----")]
    public GameObject playerDmgFlash; // UI element when player takes damage
    public GameObject pauseMenu; //UI element for the pause screen

    public bool isPaused;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this; // Binds the singleton on startup
        player = GameObject.FindGameObjectWithTag("Player"); // Find the Player & bind
        playerScript = player.GetComponent<playerController>(); // Find the playerController & bind
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Cancel"))
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
}
