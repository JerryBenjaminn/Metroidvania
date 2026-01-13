using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    [Header("Cursor")]
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;
    public CursorMode mode = CursorMode.Auto;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Apply();
    }

    void OnEnable() => Apply();

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) Apply();
    }

    void Apply()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(cursorTexture, hotspot, mode);
    }
}
