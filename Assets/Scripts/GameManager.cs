using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] HUD hud;
    [SerializeField] ShipController ship;
    [SerializeField] InvaderFleet fleet;

    public static event Action<float> OnTimeChanged;

    bool isPlaying;
    float timer;
    float bestTime;

    const string BestTimeKey = "BestTime";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else
        {
            Destroy(gameObject);
            return;
        }
        
        bestTime = PlayerPrefs.GetFloat(BestTimeKey, 0f);
    }

    void Start()
    {
        hud.ShowMainMenu(bestTime);
    }

    void Update()
    {
        if (!isPlaying) return;
        timer += Time.deltaTime;
        OnTimeChanged?.Invoke(timer);
        // hud.UpdateTimer(timer);
    }

    public void StartGame()
    {
        isPlaying = true;
        timer = 0f;
        ship.Activate();
        fleet.SpawnFleet();
        hud.ShowGamePanel();
    }

    public void RestartGame()
    {
        fleet.ClearFleet();
        StartGame();
    }

    public void OnAllInvadersDestroyed()
    {
        if (!isPlaying) return;
        isPlaying = false;
        ship.Deactivate();

        bool isNewRecord = bestTime <= 0f || timer < bestTime;
        if (isNewRecord)
        {
            bestTime = timer;
            PlayerPrefs.SetFloat(BestTimeKey, bestTime);
            PlayerPrefs.Save();
        }

        hud.ShowWinPanel(timer, isNewRecord, bestTime);
    }

    public void OnFleetReachedPlayer()
    {
        if (!isPlaying) return;
        isPlaying = false;
        ship.Deactivate();
        hud.ShowLosePanel();
    }
}
