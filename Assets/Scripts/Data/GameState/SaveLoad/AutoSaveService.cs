using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSaveService : MonoBehaviour
{
    public float intervalSeconds = 60f;
    private float nextAt;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
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
    }

    void OnApplicationPause(bool pause)
    {
        var sm = SaveManager.Instance;
        if (pause && sm && sm.HasActiveSlot && !sm.IsLoading && !sm.SavesDisabled)
            sm.SaveNow();
    }

    void OnApplicationQuit()
    {
        var sm = SaveManager.Instance;
        if (sm && sm.HasActiveSlot && !sm.IsLoading && !sm.SavesDisabled)
            sm.SaveNow();
    }
}
