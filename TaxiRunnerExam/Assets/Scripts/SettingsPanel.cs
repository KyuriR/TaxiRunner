using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public Slider sfxSlider;
    public Slider musicSlider;

    void OnEnable()
    {
        if (AudioManager.Instance == null) return;

        // Set sliders to current volume when panel opens
        if (sfxSlider != null)
            sfxSlider.value = AudioManager.Instance.sfxVolume;

        if (musicSlider != null)
            musicSlider.value = AudioManager.Instance.musicVolume;
    }

    public void OnSFXChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    public void OnMusicChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }
}