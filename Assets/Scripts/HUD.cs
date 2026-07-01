using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject gamePanel;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;

    [SerializeField] TextMeshProUGUI mainMenuBestTimeText;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] TextMeshProUGUI reloadingText;
    [SerializeField] TextMeshProUGUI winYourTimeText;
    [SerializeField] TextMeshProUGUI winBestTimeText;
    [SerializeField] TextMeshProUGUI newRecordText;


    private void OnEnable() {
        ShipController.OnAmmoChanged += OnAmmoChanged;
        ShipController.OnReloadingChanged += OnReloadingChanged;
        GameManager.OnTimeChanged += UpdateTimer;
    }

    private void OnDisable() {
        ShipController.OnAmmoChanged -= OnAmmoChanged;
        ShipController.OnReloadingChanged -= OnReloadingChanged;
        GameManager.OnTimeChanged -= UpdateTimer;
    }

    public void ShowMainMenu(float bestTime)
    {
        SetPanels(true, false, false, false);
        mainMenuBestTimeText.text = bestTime <= 0f ? "Best: --" : "Best: " + FormatTime(bestTime);
    }

    public void ShowGamePanel()
    {
        SetPanels(false, true, false, false);
        reloadingText.gameObject.SetActive(false);
    }

    public void ShowWinPanel(float time, bool isNewRecord, float bestTime)
    {
        SetPanels(false, false, true, false);
        winYourTimeText.text = "Your Time: " + FormatTime(time);
        winBestTimeText.text = "Best Time: " + FormatTime(bestTime);
        newRecordText.gameObject.SetActive(isNewRecord);
    }

    public void ShowLosePanel()
    {
        SetPanels(false, false, false, true);
    }

    private void UpdateTimer(float time)
    {
        timerText.text = FormatTime(time);
    }

    // public void UpdateTimer(float time)
    // {
    //     timerText.text = FormatTime(time);
    // }

    private void OnAmmoChanged(int current, int max)
    {
        ammoText.text = "Ammo: " + current + "/" + max;
    }

    // public void UpdateAmmo(int current, int max)
    // {
    //     ammoText.text = "Ammo: " + current + "/" + max;
    // }

    private void OnReloadingChanged(bool reloading)
    {
        reloadingText.gameObject.SetActive(reloading);
    }

    // public void SetReloading(bool reloading)
    // {
    //     reloadingText.gameObject.SetActive(reloading);
    // }

    void SetPanels(bool menu, bool game, bool win, bool lose)
    {
        mainMenuPanel.SetActive(menu);
        gamePanel.SetActive(game);
        winPanel.SetActive(win);
        losePanel.SetActive(lose);
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int centiseconds = Mathf.FloorToInt((time * 100f) % 100f);
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
    }
}
