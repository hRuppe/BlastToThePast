using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class menuController : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject settingsMenu;

    [SerializeField] AudioMixer mixer; 
    
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    [SerializeField] AudioSource sfxSource;

    [SerializeField] Button[] interactiveButtons; 

    public void openSettings()
    {
        settingsMenu.SetActive(true);
        mainMenu.SetActive(false);  
    }

    public void closeSettings()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);  
    }

    public void MasterSliderChanged()
    {
        if (masterSlider.value <= 0)
            mixer.SetFloat("Master Volume", -80.0f);
        else 
            mixer.SetFloat("Master Volume", Mathf.Log10(masterSlider.value) * 20); 
    }

    public void MusicSliderChanged()
    {
        if (musicSlider.value <= 0)
            mixer.SetFloat("Music Volume", -80.0f);
        else
            mixer.SetFloat("Music Volume", Mathf.Log10(musicSlider.value) * 20);
    }

    public void SFXSliderChanged()
    {
        if (sfxSlider.value <= 0)
            mixer.SetFloat("SFX Volume", -80.0f);
        else
            mixer.SetFloat("SFX Volume", Mathf.Log10(sfxSlider.value) * 20);
    }

    public void SFXTestPlay()
    {
        sfxSource.Play(); 
    }

}
