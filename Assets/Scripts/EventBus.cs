using System;

public static class EventBus
{
    // Game flow — published by GameControl, consumed by HUD
    public static event Action<float> OnMainMenuOpened;
    public static event Action OnGameStarted;
    public static event Action<float> OnTimerUpdated;
    public static event Action<float, bool, float> OnGameWon;   // time, isNewRecord, bestTime
    public static event Action OnGameLost;

    // Ship state — published by ShipController, consumed by HUD
    public static event Action<int, int> OnAmmoChanged;         // current, max
    public static event Action<bool> OnReloadingChanged;

    // Fleet outcome — published by InvaderFleet, consumed by GameControl
    public static event Action OnAllInvadersDestroyed;
    public static event Action OnFleetReachedPlayer;

    public static void PublishMainMenuOpened(float bestTime)                            => OnMainMenuOpened?.Invoke(bestTime);
    public static void PublishGameStarted()                                             => OnGameStarted?.Invoke();
    public static void PublishTimerUpdated(float time)                                  => OnTimerUpdated?.Invoke(time);
    public static void PublishGameWon(float time, bool isNewRecord, float bestTime)     => OnGameWon?.Invoke(time, isNewRecord, bestTime);
    public static void PublishGameLost()                                                => OnGameLost?.Invoke();
    public static void PublishAmmoChanged(int current, int max)                         => OnAmmoChanged?.Invoke(current, max);
    public static void PublishReloadingChanged(bool isReloading)                        => OnReloadingChanged?.Invoke(isReloading);
    public static void PublishAllInvadersDestroyed()                                    => OnAllInvadersDestroyed?.Invoke();
    public static void PublishFleetReachedPlayer()                                      => OnFleetReachedPlayer?.Invoke();
}
