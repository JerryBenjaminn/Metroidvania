using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private Checkpoint lastCheckpoint;
    public int CurrentSlot { get; private set; } = -1;
    public bool HasActiveSlot => CurrentSlot >= 0;
    public bool IsLoading { get; private set; }
    public bool SavesDisabled { get; set; } // esim. päävalikossa true
    public int slotCount = 3;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame(Checkpoint checkpoint)
    {
        lastCheckpoint = checkpoint;

        //Tallenna pelaajan tila (positio, hp yms)
        PlayerSave playerSave = FindFirstObjectByType<PlayerSave>();
       
        Debug.Log("Game saved at checkpoint: " + checkpoint.name);
    }

    public void LoadGame()
    {
        if (lastCheckpoint != null)
        {
            //Palautetaan pelaajan tila
            PlayerSave playerSave = FindFirstObjectByType<PlayerSave>();
            /*if(playerSave != null)
            {
                playerSave.RestoreStateFromCheckpoint(lastCheckpoint);
            }*/

            Debug.Log("Game loaded at checkpoint: " + lastCheckpoint.name);
        }
        else
        {
            Debug.LogWarning("No checkpoint found to load from!");
        }
    }

    string PathForSlot(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

    // ---------- Public API ----------
    public void SetActiveSlot(int slot) => CurrentSlot = slot;

    public bool HasSave(int slot) => File.Exists(PathForSlot(slot));

    public void DeleteSlot(int slot)
    {
        var p = PathForSlot(slot);
        if (File.Exists(p)) File.Delete(p);
    }

    public void SaveNow()
    {
        if (!HasActiveSlot) { Debug.LogWarning("[Save] skipped: no active slot"); return; }
        if (IsLoading) { Debug.Log("[Save] skipped: loading"); return; }
        if (SavesDisabled) { Debug.Log("[Save] skipped: disabled"); return; }

        //Skip saving if the active scene is the Main Menu
        if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            Debug.Log("[Save] skipped: Main Menu scene");
            return;
        }

        Debug.Log("[SaveNow] Triggered");
        var data = Capture();
        var json = JsonUtility.ToJson(data, true);
        WriteJsonAtomic(CurrentSlot, json);
        Debug.Log("[Save] wrote slot " + CurrentSlot);
    }


    public void NewGame(int slot, string startScene, Vector3 startPos)
    {
        SetActiveSlot(slot);

        // Aloitetaan puhtaalta pöydältä (jos haluat pitää vanhan saven, jätä Delete pois)
        DeleteSlot(slot);

        // Valikko-tilassa: estä tallennus siirtymän aikana
        IsLoading = true;
        SavesDisabled = true;

        GameManager.Instance.StartCoroutine(
            CoLoadSceneAndPlace(startScene, startPos, writeInitialSave: true)
        );
    }

    // SaveManager.cs
    public void LoadSlot(int slot)
    {
        var path = PathForSlot(slot);
        if (!File.Exists(path)) { Debug.LogWarning($"[Save] No save in slot {slot}"); return; }

        var json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<SaveFile>(json);
        if (data == null || string.IsNullOrEmpty(data.scene) || !Application.CanStreamedLevelBeLoaded(data.scene))
        {
            Debug.LogWarning($"[Save] Invalid save in slot {slot}"); return;
        }

        SetActiveSlot(slot);
        IsLoading = true;           // LUKKO PÄÄLLE
        StartCoroutine(CoLoadSceneAndApply(data));
    }



    // ---------- Capture & Apply ----------
    SaveFile Capture()
    {
        var data = new SaveFile();
        var scene = SceneManager.GetActiveScene();
        data.scene = scene.name;
        data.version = 1;
        data.savedAtUtcTicks = System.DateTime.UtcNow.Ticks;

        // Pelaaja
        var player = GameManager.Instance.Player;
        if (player)
        {
            var hp = player.GetComponent<Health>();
            var ac = player.GetComponent<AbilityController>();
            data.player = new PlayerState
            {
                pos = player.position,
                health = hp ? hp.Current : 0
            };
            if (ac) data.unlockedAbilities = ac.GetUnlockedAbilityNames();
        }

        // Scene-entiteetit
        //var all = GameObject.FindObjectsOfType<SaveableEntity>(includeInactive: true);
        // jälkeen, tuplapolku:
#if UNITY_2023_1_OR_NEWER
        var all = Object.FindObjectsByType<SaveableEntity>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
#else
var all = GameObject.FindObjectsOfType<SaveableEntity>(true);
#endif

        data.entities = new List<EntityRecord>(all.Length);
        foreach (var e in all)
        {
            var comps = e.GetSaveables();
            if (comps == null || comps.Length == 0) continue;

            var rec = new EntityRecord { id = e.Id };
            var types = new List<string>();
            var states = new List<string>();

            foreach (var c in comps)
            {
                var stateObj = c.CaptureState();
                if (stateObj == null) continue;
                types.Add(c.GetType().AssemblyQualifiedName);
                states.Add(JsonUtility.ToJson(stateObj));
            }
            rec.componentTypes = types.ToArray();
            rec.componentStates = states.ToArray();
            data.entities.Add(rec);
        }
        return data;
    }

    IEnumerator CoLoadSceneAndApply(SaveFile data)
    {
        Debug.Log("[Save] Applying save...");
        // 1) Lataa scene tarvittaessa
        var active = SceneManager.GetActiveScene().name;
        if (data.scene != active)
        {
            var op = SceneManager.LoadSceneAsync(data.scene, LoadSceneMode.Single);
            while (!op.isDone) yield return null;
        }

        // 2) Aseta pelaaja
        var player = GameManager.Instance.Player;
        if (player && data.player != null)
        {
            player.position = data.player.pos;

            var hp = player.GetComponent<Health>();

            if (hp)
            {
                hp.SetHealthFromSave(data.player.health);
                hp.ForceInvulnerability(0.25f);
            }

            var ac = player.GetComponent<AbilityController>();
            if (ac != null && data.unlockedAbilities != null)
                ac.SetUnlockedByNames(data.unlockedAbilities);
        }

        // 3) Palauta entiteettien tila
        var map = new Dictionary<string, SaveableEntity>();
        //foreach (var e in GameObject.FindObjectsOfType<SaveableEntity>(includeInactive: true))
           // map[e.Id] = e;
#if UNITY_2023_1_OR_NEWER
        var ents = Object.FindObjectsByType<SaveableEntity>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
#else
var ents = GameObject.FindObjectsOfType<SaveableEntity>(true);
#endif
        foreach (var e in ents) map[e.Id] = e;

        foreach (var rec in data.entities)
        {
            if (!map.TryGetValue(rec.id, out var ent)) continue;
            var comps = ent.GetSaveables();
            // tee indeksi tyypeistä, jotta järjestys ei ole kriittinen
            var typeMap = new Dictionary<string, ISaveable>();
            foreach (var c in comps) typeMap[c.GetType().AssemblyQualifiedName] = c;

            for (int i = 0; i < rec.componentTypes.Length; i++)
            {
                var tKey = rec.componentTypes[i];
                var json = rec.componentStates[i];
                if (!typeMap.TryGetValue(tKey, out var target)) continue;

                // selvitä state-tyyppi heijastuksella
                var t = target.GetType().GetNestedType("State", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                // fallback: yritä deserialisoida suoraan objectiksi (vaatii että CaptureState palauttaa konkreettisen tyypin)
                object stateObj = null;
                try { stateObj = JsonUtility.FromJson(json, target.CaptureState().GetType()); }
                catch { if (t != null) stateObj = JsonUtility.FromJson(json, t); }

                if (stateObj != null) target.RestoreState(stateObj);
            }
        }
        Debug.Log("[Save] Scene loaded, placing player, restoring entities");
        IsLoading = false;
        SavesDisabled = false;
    }


    IEnumerator CoLoadSceneAndPlace(string sceneName, Vector3 pos, bool writeInitialSave)
    {
        var op = UnityEngine.SceneManagement.SceneManager
            .LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        while (!op.isDone) yield return null;

        var player = GameManager.Instance.Player;
        if (player) player.position = pos;

        // Ensure saving is skipped when entering the GameScene
        if (writeInitialSave && sceneName != "GameScene")
        {
            yield return null;
            var data = Capture();
            var json = JsonUtility.ToJson(data, true);
            WriteJsonAtomic(CurrentSlot, json);
        }

        // Allow saving after the scene has been loaded
        IsLoading = false;
        SavesDisabled = false;
    }
    void WriteJsonAtomic(int slot, string json)
    {
        var path = PathForSlot(slot);
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, json);
        if (File.Exists(path)) File.Delete(path);
        File.Move(tmp, path);
    }
    // SaveManager.cs
    /*public bool HasValidSave(int slot)
    {
        var path = PathForSlot(slot);
        if (!File.Exists(path)) return false;
        try
        {
            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveFile>(json);
            return data != null
                && !string.IsNullOrEmpty(data.scene)
                && Application.CanStreamedLevelBeLoaded(data.scene);
        }
        catch { return false; }
    }*/



    // ---------- Data types ----------
    [System.Serializable]
    public class SaveFile
    {
        public int version;
        public string scene;
        public PlayerState player;
        public List<string> unlockedAbilities;
        public List<EntityRecord> entities;
        public string playerSpawnScene;
        public Vector3 playerSpawnPos;

        //Metadata
        public long savedAtUtcTicks; //aikaleima
    }

    [System.Serializable]
    public struct SaveSummary
    {
        public bool exists;
        public bool valid;
        public string scene;
        public float hp;
        public int abilityCount;
        public System.DateTime savedAtUtc;
    }

    public SaveSummary GetSummary(int slot)
    {
        var sum = new SaveSummary { exists = false, valid = false, scene = "", hp = 0, abilityCount = 0, savedAtUtc = System.DateTime.MinValue };

        var path = PathForSlot(slot);
        if (!System.IO.File.Exists(path)) return sum;

        sum.exists = true;
        try
        {
            var json = System.IO.File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveFile>(json);
            if (data == null || string.IsNullOrEmpty(data.scene)) return sum;
            if (!Application.CanStreamedLevelBeLoaded(data.scene)) return sum;

            sum.valid = true;
            sum.scene = data.scene;
            sum.hp = data.player != null ? data.player.health : 0;
            sum.abilityCount = data.unlockedAbilities != null ? data.unlockedAbilities.Count : 0;
            sum.savedAtUtc = data.savedAtUtcTicks != 0
                ? new System.DateTime(data.savedAtUtcTicks, System.DateTimeKind.Utc)
                : System.DateTime.MinValue;

            return sum;
        }
        catch
        {
            return sum; // exists = true, valid = false
        }
    }
    public bool HasValidSave(int slot) => GetSummary(slot).valid;

    [System.Serializable]
    public class PlayerState
    {
        public Vector3 pos;
        public float health;
    }

    [System.Serializable]
    public class EntityRecord
    {
        public string id;
        public string[] componentTypes;
        public string[] componentStates;
    }
}
