using UnityEngine;

public class GameControl : MonoBehaviour
{
    public static event System.Action<float> OnGameShowMainMenu;
    public static event System.Action OnGameStart;
    public static event System.Action<float> OnGameUpdateTimer;
    public static event System.Action OnGameLose;
    public static event System.Action<float, bool, float> OnGameWin;
    // Singleton instance
    public static GameControl instance;

    // [SerializeField] HUD hud;
    [SerializeField] ShipController ship;
    [SerializeField] InvaderFleet fleet;

    bool isPlaying;
    float timer;
    float bestTime;

    const string BestTimeKey = "BestTime";

    public static GameControl Instance => instance;

    void Awake()
    {
        // supaya cuma ada 1 GameControl aktif di scene
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set object ini sebagai satu-satunya instance yang valid
        instance = this;

        bestTime = PlayerPrefs.GetFloat(BestTimeKey, 0f);
    }

    void OnDestroy()
    {
        // Bersihkan referensi singleton saat object ini di-destroy,
        // supaya gak nyisain reference ke instance yang udah gak valid
        if (instance == this)
            instance = null;
    }

    void Start()
    {
        OnGameShowMainMenu?.Invoke(bestTime); // hud.ShowMainMenu(bestTime);
    }

    void Update()
    {
        if (!isPlaying) return;
        timer += Time.deltaTime;
        OnGameUpdateTimer?.Invoke(timer); // hud.UpdateTimer(timer);
    }

    public void StartGame()
    {
        isPlaying = true;
        timer = 0f;
        if (ship != null) ship.Activate();
        if (fleet != null) fleet.SpawnFleet();
        OnGameStart?.Invoke(); // hud.ShowGamePanel();
    }

    public void RestartGame()
    {
        if (fleet != null) fleet.ClearFleet();
        StartGame();
    }

    public void OnAllInvadersDestroyed()
    {
        if (!isPlaying) return;
        isPlaying = false;
        if (ship != null) ship.Deactivate();

        bool isNewRecord = bestTime <= 0f || timer < bestTime;
        if (isNewRecord)
        {
            bestTime = timer;
            PlayerPrefs.SetFloat(BestTimeKey, bestTime);
            PlayerPrefs.Save();
        }

        OnGameWin?.Invoke(timer, isNewRecord, bestTime); // hud.ShowWinPanel(timer, isNewRecord, bestTime);
    }

    public void OnFleetReachedPlayer()
    {
        if (!isPlaying) return;
        isPlaying = false;
        if (ship != null) ship.Deactivate();
        OnGameLose?.Invoke(); // hud.ShowLosePanel();
    }
}
