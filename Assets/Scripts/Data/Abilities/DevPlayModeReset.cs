#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class DevPlaymodeReset
{
    const string ToggleKey = "Dev/ClearPlayerPrefsOnPlay";

    static DevPlaymodeReset()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(ToggleKey, false))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("[DEV] Cleared all PlayerPrefs on play.");
        }
    }

    [MenuItem("Tools/Dev/Toggle: Clear PlayerPrefs On Play")]
    static void Toggle()
    {
        bool v = !EditorPrefs.GetBool(ToggleKey, false);
        EditorPrefs.SetBool(ToggleKey, v);
        Debug.Log("[DEV] Clear PlayerPrefs On Play: " + (v ? "ON" : "OFF"));
    }

    [MenuItem("Tools/Dev/Clear PlayerPrefs Now")]
    static void ClearNow()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[DEV] Cleared all PlayerPrefs now.");
    }
}
#endif
