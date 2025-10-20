using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Transform Player { get; private set; }

    [SerializeField] SaveManager saveManagerPrefab;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // varmista että SaveManager on olemassa
        if (!SaveManager.Instance)
            Instantiate(saveManagerPrefab);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.globalSfxVolume = 0.3f; // Adjust this value as needed
        }
    }

    // kutsu kun pelaaja spawnaa / scene vaihtuu
    public void RegisterPlayer(Transform playerRoot)
    {
        Player = playerRoot;
    }

    // Menu-napit
    public void NewGame(int slot, string startScene, Vector3 startPos) =>
        SaveManager.Instance.NewGame(slot, startScene, startPos);

    public void LoadGame(int slot) => SaveManager.Instance.LoadSlot(slot);
    public void SaveGame() => SaveManager.Instance.SaveNow();
    public void DeleteSave(int slot) => SaveManager.Instance.DeleteSlot(slot);
}
