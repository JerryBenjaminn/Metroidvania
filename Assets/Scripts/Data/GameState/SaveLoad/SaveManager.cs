using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public int CurrentSlot { get; private set; } = -1;
    public bool HasActiveSlot => CurrentSlot >= 0;
    public bool IsLoading { get; private set; }
    public bool SavesDisabled { get; set; } // esim. päävalikossa true

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        if (!HasActiveSlot) return;
        if (IsLoading) return;              // älä tallenna kun luetaan!
        if (SavesDisabled) return;          // älä tallenna päävalikossa
        var data = Capture();
        var json = JsonUtility.ToJson(data, true);
        WriteJsonAtomic(CurrentSlot, json);
    }

    public void NewGame(int slot, string startScene, Vector3 startPos)
    {
        SetActiveSlot(slot);
        DeleteSlot(slot); // varmistetaan puhdas aloitus
        // Lataa aloitusscene ja spawnaa pelaaja sinne
        GameManager.Instance.StartCoroutine(CoLoadSceneAndPlace(startScene, startPos, writeInitialSave: true));
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
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!op.isDone) yield return null;

        var player = GameManager.Instance.Player;
        if (player) player.position = pos;

        if (writeInitialSave)
        {
            // anna framesta hengähdys että kaikki ehtii rekisteröityä
            yield return null;
            var data = Capture();
            var json = JsonUtility.ToJson(data, true);
            WriteJsonAtomic(CurrentSlot, json);  // katso metodi alla
        }
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
    public bool HasValidSave(int slot)
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
    }



    // ---------- Data types ----------
    [System.Serializable]
    public class SaveFile
    {
        public int version;
        public string scene;
        public PlayerState player;
        public List<string> unlockedAbilities;
        public List<EntityRecord> entities;
    }

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
