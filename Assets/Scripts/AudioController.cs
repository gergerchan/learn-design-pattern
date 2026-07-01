using UnityEngine;

class AudioController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip buttonClickAudioClip;
    public AudioClip loseAudioClip;
    public AudioClip winAudioClip;
    public AudioClip[] ammoLaunchProjectileAudioClip;
    public AudioClip ammoReloadAudioClip;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void OnEnable()
    {
        GameControl.OnGameStart += OnGameStartHandler;
        GameControl.OnGameWin += OnGameWinHandler;
        GameControl.OnGameLose += OnGameLoseHandler;
        ShipController.OnAmmoUpdate += OnAmmoUpdateHandler;
        ShipController.OnAmmoReload += OnAmmoReloadHandler;
    }

    void OnDisable()
    {
        GameControl.OnGameStart -= OnGameStartHandler;
        GameControl.OnGameWin -= OnGameWinHandler;
        GameControl.OnGameLose -= OnGameLoseHandler;
        ShipController.OnAmmoUpdate -= OnAmmoUpdateHandler;
        ShipController.OnAmmoReload -= OnAmmoReloadHandler;
    }

    void PlayAudio(AudioClip audio)
    {
        audioSource.PlayOneShot(audio);
    }

    void StopAudio()
    {
        audioSource.Stop();
    }

    void OnGameStartHandler()
    {
        PlayAudio(buttonClickAudioClip);
    }

    void OnGameWinHandler(float gameTime, bool isNewRecord, float bestTime)
    {
        PlayAudio(winAudioClip);
    }

    void OnGameLoseHandler()
    {
        PlayAudio(loseAudioClip);
    }

    void OnAmmoUpdateHandler(int bulletsRemaining, int maxAmmo)
    {
        if (bulletsRemaining > 0)
        {
            PlayAudio(ammoLaunchProjectileAudioClip[Random.Range(0, ammoLaunchProjectileAudioClip.Length)]);
        }
    }

    void OnAmmoReloadHandler(bool isReloading)
    {
        if (isReloading)
        {
            PlayAudio(ammoReloadAudioClip);
        }
    }
}
