// 10/7/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSaveService : MonoBehaviour
{
    public float intervalSeconds = 60f; // Optional: Keep for future use
    private float nextAt;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Disable interval-based saving
        // If you want to re-enable it in the future, uncomment the following lines:
        /*
        var sm = SaveManager.Instance;
        if (sm == null || !sm.HasActiveSlot) return;
        if (sm.IsLoading) return;
        if (sm.SavesDisabled) return;
        if (SceneManager.GetActiveScene().name == "MainMenu") return;

        if (Time.unscaledTime >= nextAt)
        {
            sm.SaveNow();
            nextAt = Time.unscaledTime + intervalSeconds;
        }
        */
    }

    void OnApplicationPause(bool pause)
    {
        // Disable saving on application pause
        /*
        var sm = SaveManager.Instance;
        if (pause && sm && sm.HasActiveSlot && !sm.IsLoading && !sm.SavesDisabled)
            sm.SaveNow();
        */
    }

    void OnApplicationQuit()
    {
        // Disable saving on application quit
        /*
        var sm = SaveManager.Instance;
        if (sm && sm.HasActiveSlot && !sm.IsLoading && !sm.SavesDisabled)
            sm.SaveNow();
        */
    }
}