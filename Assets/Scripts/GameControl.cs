using UnityEngine;

public class GameControl : MonoBehaviour
{
    public static GameControl instance;

    [SerializeField] ShipController ship;
    [SerializeField] InvaderFleet fleet;

    bool isPlaying;
    float timer;
    float bestTime;

    const string BestTimeKey = "BestTime";

    void Awake()
    {
        instance = this;
        bestTime = PlayerPrefs.GetFloat(BestTimeKey, 0f);
        EventBus.OnAllInvadersDestroyed += HandleAllInvadersDestroyed;
        EventBus.OnFleetReachedPlayer += HandleFleetReachedPlayer;
    }

    void OnDestroy()
    {
        EventBus.OnAllInvadersDestroyed -= HandleAllInvadersDestroyed;
        EventBus.OnFleetReachedPlayer -= HandleFleetReachedPlayer;
    }

    void Start()
    {
        EventBus.PublishMainMenuOpened(bestTime);
    }

    void Update()
    {
        if (!isPlaying) return;
        timer += Time.deltaTime;
        EventBus.PublishTimerUpdated(timer);
    }

    public void StartGame()
    {
        isPlaying = true;
        timer = 0f;
        ship.Activate();
        fleet.SpawnFleet();
        EventBus.PublishGameStarted();
    }

    public void RestartGame()
    {
        fleet.ClearFleet();
        StartGame();
    }

    void HandleAllInvadersDestroyed()
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

        EventBus.PublishGameWon(timer, isNewRecord, bestTime);
    }

    void HandleFleetReachedPlayer()
    {
        if (!isPlaying) return;
        isPlaying = false;
        ship.Deactivate();
        EventBus.PublishGameLost();
    }
}
