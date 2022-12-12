using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class menuController : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject creditsMenu;
    [SerializeField] GameObject controlsMenu;
    [SerializeField] Image bttpLogo;

    [SerializeField] AudioClip btnHoverClip;
    [SerializeField] AudioClip btnClickClip;

    [SerializeField] AudioMixer mixer; 
    
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    [SerializeField] AudioSource sfxSource;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            masterSlider.value = PlayerPrefs.GetFloat("MasterVolume");
            MasterSliderChanged();
        }

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
            MusicSliderChanged();
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
            SFXSliderChanged();
        }
    }

    private void Update()
    {
        OpenShowCaseLevel(); 
    }

    public void openSettings()
    {
        settingsMenu.SetActive(true);
        mainMenu.SetActive(false);  
    }

    public void closeOverlapMenu()
    {
        if (!bttpLogo.IsActive()) bttpLogo.gameObject.SetActive(true); 
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(false); 
    }

    public void openCredits()
    {
        creditsMenu.SetActive(true);
        mainMenu.SetActive(false);
        bttpLogo.gameObject.SetActive(false); 
    }

    public void OpenControlsMenu()
    {
        controlsMenu.SetActive(true);
        mainMenu.SetActive(false);
        bttpLogo.gameObject.SetActive(false); 
    }

    public void CloseControlsMenu()
    {
        controlsMenu.SetActive(false);
        mainMenu.SetActive(true);
        bttpLogo.gameObject.SetActive(true); 
    }

    public void MasterSliderChanged()
    {
        if (masterSlider.value <= 0)
            mixer.SetFloat("Master Volume", -80.0f);
        else 
            mixer.SetFloat("Master Volume", Mathf.Log10(masterSlider.value) * 20);

        PlayerPrefs.SetFloat("MasterVolume", masterSlider.value); 
    }

    public void MusicSliderChanged()
    {
        if (musicSlider.value <= 0)
            mixer.SetFloat("Music Volume", -80.0f);
        else
            mixer.SetFloat("Music Volume", Mathf.Log10(musicSlider.value) * 20);

        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
    }

    public void SFXSliderChanged()
    {
        if (sfxSlider.value <= 0)
            mixer.SetFloat("SFX Volume", -80.0f);
        else
            mixer.SetFloat("SFX Volume", Mathf.Log10(sfxSlider.value) * 20);

        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
    }

    public void SFXTestPlay()
    {
        sfxSource.Play(); 
    }

    public void PlayBtnHover()
    {
        sfxSource.PlayOneShot(btnHoverClip); 
    }

    public void PlayBtnClick()
    {
        sfxSource.PlayOneShot(btnClickClip);
    }

    void OpenShowCaseLevel()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene("Showcase Level"); 
        }
    }
}
