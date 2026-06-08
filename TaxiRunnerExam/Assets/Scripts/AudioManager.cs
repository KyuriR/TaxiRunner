using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Sound Effects")]
    public AudioClip cashSound;       // passenger collected / fare earned
    public AudioClip tipSound;        // successful drop-off tip
    public AudioClip crashSound;      // car collision
    public AudioClip potholeSound;    // pothole hit
    public AudioClip policeSound;     // police siren warning
    public AudioClip buttonSound;     // menu button press
    public AudioClip closeSound;      // close/end game button
    public AudioClip gameOverSound;   // game over

    [Header("Music")]
    public AudioClip backgroundMusic; // driving background music

    [Header("Volume")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Before destroying, copy clips to the persisted instance
            // in case this new one has clips assigned
            if (cashSound != null) Instance.cashSound = cashSound;
            if (tipSound != null) Instance.tipSound = tipSound;
            if (crashSound != null) Instance.crashSound = crashSound;
            if (potholeSound != null) Instance.potholeSound = potholeSound;
            if (policeSound != null) Instance.policeSound = policeSound;
            if (buttonSound != null) Instance.buttonSound = buttonSound;
            if (closeSound != null) Instance.closeSound = closeSound;
            if (gameOverSound != null) Instance.gameOverSound = gameOverSound;
            if (backgroundMusic != null) Instance.backgroundMusic = backgroundMusic;

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", sfxVolume);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", musicVolume);

        if (sfxSource != null)
            sfxSource.volume = 1f;

        if (musicSource != null)
            musicSource.volume = musicVolume;

        PlayMusic();
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (musicSource != null && !musicSource.isPlaying)
            PlayMusic();
    }
    public void PlayCash()
    {
        Play(cashSound);
    }

    public void PlayTip()
    {
        Play(tipSound);
    }

    public void PlayCrash()
    {
        Play(crashSound);
    }

    public void PlayPothole()
    {
        Play(potholeSound);
    }

    public void PlayPolice()
    {
        Play(policeSound);
    }

    public void PlayButton()
    {
        Play(buttonSound);
    }

    public void PlayClose()
    {
        Play(closeSound);
    }

    public void PlayGameOver()
    {
        Play(gameOverSound);
    }

    void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        // Keep the source at full volume and control each sound with the multiplier
        sfxSource.volume = 1f;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ── Music ─────────────────────────────────────────────────────────────────

    public void PlayMusic()
    {
        if (backgroundMusic == null || musicSource == null) return;
        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        if (musicSource == null) return;
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource == null) return;
        musicSource.UnPause();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();

        if (sfxSource != null)
            sfxSource.volume = 1f;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();

        if (musicSource != null)
            musicSource.volume = musicVolume;
    }
}
