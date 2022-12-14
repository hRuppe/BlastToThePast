using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class dontDestroy : MonoBehaviour
{
    [SerializeField] AudioSource source; 
    [SerializeField] AudioClip gameplayMusic;

    bool newMusicStarted; 

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!newMusicStarted && SceneManager.GetActiveScene().buildIndex == 1)
        {
            PlayGameplayMusic(); 
        }
    }

    // Changes the music when you start playing the game
    void PlayGameplayMusic()
    {
        source.clip = gameplayMusic;
        source.volume = .175f; // gameplay music is way louder so this balances it out
        source.Play();
        newMusicStarted = true;
    }
}
