#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ExportSceneContext
{
    // ===== ASETUKSET =====
    const string OutputPath = "Temp/ai_context.json";

    // Snapshot-kokonaisraja
    const int MaxSnapshotChars = 350_000;

    // Skriptit
    const int MaxScripts = 80;
    const int MaxScriptCharsPerFile = 20_000;
    const int MaxScriptCharsTotal = 200_000;

    // ScriptableObjectit
    const int MaxScriptableObjects = 80;          // maks. SO:ita snapshotissa
    const int MaxSoFieldsPerObject = 200;         // maks. tallennettavia kenttiä / SO
    const int MaxSoCharsTotal = 120_000;          // kaikkien SO-tiivistelmien yhteismaksimi

    // Voit pakottaa mukaan tietyt SO-tyypit nimellä (esim. PlayerStats, InventoryConfig)
    static readonly string[] ForceIncludeSoTypeNames = { /* "PlayerConfig", "InventoryDB" */ };

    // ===== MENU =====
    [MenuItem("AI/Export Scene Context (JSON)")]
    public static void Export()
    {
        try
        {
            var ctx = BuildProjectContext();
            var json = ToPrettyJson(ctx);

            if (json.Length > MaxSnapshotChars)
            {
                // Karsi ensin skriptit ja SO:t, jos liian iso
                var slim = BuildProjectContext(withScripts: false, withScriptables: false);
                json = ToPrettyJson(slim);
                if (json.Length > MaxSnapshotChars)
                    json = json.Substring(0, MaxSnapshotChars) + "\n/* ...truncated... */";
            }

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath) ?? "Temp");
            File.WriteAllText(OutputPath, json, new UTF8Encoding(false));
            Debug.Log($"[AI] Context exported -> {Path.GetFullPath(OutputPath)}  ({json.Length:n0} chars)");
            EditorUtility.RevealInFinder(OutputPath);
        }
        catch (Exception e)
        {
            Debug.LogError("[AI] Export failed:\n" + e);
        }
    }

    [MenuItem("AI/Export Scene Context (JSON) Auto/Enable")]
    public static void EnableAutoExport()
    {
        Export();
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorSceneManager.sceneClosed -= OnSceneClosed;
        EditorSceneManager.sceneClosed += OnSceneClosed;
        Debug.Log("[AI] Auto-export enabled (on hierarchy/scene change).");
    }

    [MenuItem("AI/Export Scene Context (JSON) Auto/Disable")]
    public static void DisableAutoExport()
    {
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.sceneClosed -= OnSceneClosed;
        Debug.Log("[AI] Auto-export disabled.");
    }

    static void OnHierarchyChanged() => Export();
    static void OnSceneOpened(Scene s, OpenSceneMode m) => Export();
    static void OnSceneClosed(Scene s) => Export();

    // ===== RAKENNUS =====
    static ProjectContext BuildProjectContext(bool withScripts = true, bool withScriptables = true)
    {
        var ctx = new ProjectContext
        {
            unityVersion = Application.unityVersion,
            generatedAt = DateTime.UtcNow.ToString("o"),
            scenes = new List<SceneContext>(),
            scriptablePaths = new List<string>(),     // polkulista säilyy
            scripts = new List<ScriptInfo>(),
            scriptables = new List<ScriptableObjectInfo>() // uudet tiivistelmät
        };

        var usedComponentTypes = new HashSet<string>(StringComparer.Ordinal);
        var referencedSOs = withScriptables ? new HashSet<ScriptableObject>() : null;

        // Scenet + hierarkia
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scn = SceneManager.GetSceneAt(i);
            if (!scn.isLoaded) continue;

            var sceneCtx = new SceneContext
            {
                name = scn.name,
                path = scn.path,
                roots = new List<GameObjectContext>()
            };

            foreach (var root in scn.GetRootGameObjects())
            {
                sceneCtx.roots.Add(DumpGO(root, null, usedComponentTypes, referencedSOs));
            }

            ctx.scenes.Add(sceneCtx);
        }

        // SO-polut (vanha listaus säilytetään referenssiksi)
        try
        {
            var soGuids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (var guid in soGuids.Take(2000))
                ctx.scriptablePaths.Add(AssetDatabase.GUIDToAssetPath(guid));
        }
        catch { /* ei kriittinen */ }

        // Skriptit: vain sceneissä käytetyt komponenttiluokat (MonoBehaviour-tyypit)
        if (withScripts && usedComponentTypes.Count > 0)
            AddScriptsForUsedTypes(ctx, usedComponentTypes);

        // ScriptableObject-tiivistelmät (sceneissä oikeasti referoidut + force-include-tyypit)
        if (withScriptables)
            AddReferencedScriptableObjects(ctx, referencedSOs);

        return ctx;
    }

    static GameObjectContext DumpGO(GameObject go, string parentPath, HashSet<string> usedComponentTypes, HashSet<ScriptableObject> referencedSOs)
    {
        var path = string.IsNullOrEmpty(parentPath) ? go.name : parentPath + "/" + go.name;

        var comps = go.GetComponents<Component>()
            .Where(c => c != null)
            .Select(c => c.GetType().Name)
            .Distinct()
            .ToList();

        foreach (var t in comps) usedComponentTypes.Add(t);

        // Kerää ScriptableObject-viittaukset SerializedObjectin kautta
        if (referencedSOs != null)
            CollectScriptableReferences(go, referencedSOs);

        var ctx = new GameObjectContext
        {
            name = go.name,
            path = path,
            tag = go.tag,
            layer = LayerMask.LayerToName(go.layer),
            active = go.activeSelf,
            prefab = GetPrefabStatus(go),
            components = comps,
            localPosition = go.transform.localPosition,
            localRotation = go.transform.localRotation.eulerAngles,
            localScale = go.transform.localScale,
            children = new List<GameObjectContext>()
        };

        for (int i = 0; i < go.transform.childCount; i++)
        {
            var ch = go.transform.GetChild(i).gameObject;
            ctx.children.Add(DumpGO(ch, path, usedComponentTypes, referencedSOs));
        }

        return ctx;
    }

    // Kerää kaikki ScriptableObject-viittaukset GameObjectin komponenteista
    static void CollectScriptableReferences(GameObject go, HashSet<ScriptableObject> set)
    {
        var comps = go.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            try
            {
                var so = new SerializedObject(c);
                var it = so.GetIterator();
                bool enterChildren = true;
                while (it.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                    var obj = it.objectReferenceValue;
                    if (obj is ScriptableObject sObj)
                    {
                        // vain assetit, ei scene-instanssit
                        var path = AssetDatabase.GetAssetPath(sObj);
                        if (!string.IsNullOrEmpty(path))
                            set.Add(sObj);
                    }
                    else if (it.isArray && it.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        // Unityn SerializedProperty ei aina listaa arrayn lapsia ilman lisäiteraatiota,
                        // mutta käytännössä ylläoleva riittää useimpiin tapauksiin.
                    }
                }
            }
            catch { /* jotkut komponentit voivat epäonnistua serialisoinnissa, ignoraa */ }
        }
    }

    // Prefab-tila ilman vanhentuneita tarkistuksia
    static string GetPrefabStatus(GameObject go)
    {
#if UNITY_2021_3_OR_NEWER
        if (PrefabUtility.IsPartOfPrefabAsset(go))
            return "PrefabAsset";

        if (PrefabUtility.IsPartOfPrefabInstance(go))
        {
            var status = PrefabUtility.GetPrefabInstanceStatus(go);
            if (status == PrefabInstanceStatus.Connected) return "PrefabInstance";
            if (status == PrefabInstanceStatus.MissingAsset) return "PrefabInstance(Missing)";
            return "PrefabInstance";
        }
        return "None";
#else
        return PrefabUtility.GetPrefabParent(go) ? "PrefabInstance" :
               PrefabUtility.GetPrefabObject(go) ? "PrefabAsset" : "None";
#endif
    }

    static void AddScriptsForUsedTypes(ProjectContext ctx, HashSet<string> usedTypes)
    {
        try
        {
            var selected = new List<ScriptInfo>();
            int totalChars = 0;

            var guids = AssetDatabase.FindAssets("t:MonoScript");
            foreach (var guid in guids)
            {
                if (selected.Count >= MaxScripts) break;

                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (ms == null) continue;

                var type = ms.GetClass();
                if (type == null) continue;

                if (!usedTypes.Contains(type.Name))
                    continue;

                string code;
                try { code = File.ReadAllText(path); } catch { continue; }

                if (code.Length > MaxScriptCharsPerFile)
                    code = code.Substring(0, MaxScriptCharsPerFile) + "\n/* ...truncated... */";

                if (totalChars + code.Length > MaxScriptCharsTotal)
                {
                    int remaining = Math.Max(0, MaxScriptCharsTotal - totalChars);
                    if (remaining > 500)
                    {
                        code = code.Substring(0, remaining) + "\n/* ...truncated(total)... */";
                        totalChars += code.Length;
                        selected.Add(new ScriptInfo { name = type.Name, path = path, code = code });
                    }
                    break;
                }

                totalChars += code.Length;
                selected.Add(new ScriptInfo { name = type.Name, path = path, code = code });
            }

            ctx.scripts = selected;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[AI] Script export failed, continuing without scripts:\n" + e.Message);
            ctx.scripts = new List<ScriptInfo>();
        }
    }

    static void AddReferencedScriptableObjects(ProjectContext ctx, HashSet<ScriptableObject> referenced)
    {
        try
        {
            var set = new HashSet<ScriptableObject>();
            if (referenced != null)
                foreach (var s in referenced) set.Add(s);

            // Force-include tyypit projekteista
            if (ForceIncludeSoTypeNames != null && ForceIncludeSoTypeNames.Length > 0)
            {
                var guids = AssetDatabase.FindAssets("t:ScriptableObject");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (so == null) continue;
                    var t = so.GetType().Name;
                    if (ForceIncludeSoTypeNames.Any(n => string.Equals(n, t, StringComparison.OrdinalIgnoreCase)))
                        set.Add(so);
                }
            }

            // Rajoitukset ja serialisointi
            int totalChars = 0;
            foreach (var so in set.Take(MaxScriptableObjects))
            {
                var info = DumpScriptableObject(so, MaxSoFieldsPerObject, ref totalChars);
                if (info != null)
                {
                    ctx.scriptables.Add(info);
                    if (totalChars >= MaxSoCharsTotal) break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[AI] SO export failed, continuing without SO details:\n" + e.Message);
            ctx.scriptables = new List<ScriptableObjectInfo>();
        }
    }

    // Tiivistä ScriptableObject: vain turvalliset skalaari- ja vektorikentät merkkijonoiksi
    static ScriptableObjectInfo DumpScriptableObject(ScriptableObject so, int maxFields, ref int totalChars)
    {
        var path = AssetDatabase.GetAssetPath(so);
        var type = so.GetType();
        var fields = new List<KV>();

        // Kerää julkaistut ja [SerializeField] kentät (instanssi)
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var flds = type.GetFields(flags)
            .Where(f =>
            {
                if (f.IsStatic) return false;
                if (f.IsPublic) return true;
                return Attribute.IsDefined(f, typeof(SerializeField));
            });

        foreach (var f in flds)
        {
            if (fields.Count >= maxFields) break;

            var val = f.GetValue(so);
            if (!TryFormatSupportedValue(val, out var s)) continue;

            var entry = new KV { key = f.Name, value = s };
            fields.Add(entry);

            totalChars += entry.key.Length + entry.value.Length + 8;
            if (totalChars >= MaxSoCharsTotal) break;
        }

        return new ScriptableObjectInfo
        {
            type = type.Name,
            name = so.name,
            path = path,
            fields = fields
        };
    }

    // Hyväksytään vain turvalliset perusarvot, ei viitteitä assetteihin
    static bool TryFormatSupportedValue(object val, out string s)
    {
        s = null;
        if (val == null) { s = "null"; return true; }

        switch (val)
        {
            case string str:
                s = str.Length > 1024 ? str.Substring(0, 1024) + "..." : str;
                return true;
            case int or uint or short or ushort or byte or sbyte or long or ulong:
            case float or double or decimal:
            case bool:
                s = Convert.ToString(val, System.Globalization.CultureInfo.InvariantCulture);
                return true;
            case Enum e:
                s = e.ToString();
                return true;
            case Vector2 v2:
                s = $"({v2.x:0.###},{v2.y:0.###})"; return true;
            case Vector3 v3:
                s = $"({v3.x:0.###},{v3.y:0.###},{v3.z:0.###})"; return true;
            case Vector4 v4:
                s = $"({v4.x:0.###},{v4.y:0.###},{v4.z:0.###},{v4.w:0.###})"; return true;
            case Color col:
                s = $"rgba({col.r:0.###},{col.g:0.###},{col.b:0.###},{col.a:0.###})"; return true;
            // Listat/taulukot perusarvoista
            default:
                var t = val.GetType();
                if (t.IsArray)
                {
                    var et = t.GetElementType();
                    if (IsPrimitiveLike(et))
                    {
                        var arr = (Array)val;
                        var items = new List<string>();
                        int count = Math.Min(arr.Length, 64);
                        for (int i = 0; i < count; i++)
                        {
                            var item = arr.GetValue(i);
                            if (!TryFormatSupportedValue(item, out var itemStr)) return false;
                            items.Add(itemStr);
                        }
                        s = "[" + string.Join(",", items) + (arr.Length > count ? ",..." : "") + "]";
                        return true;
                    }
                }
                // List<T>
                if (t.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(t))
                {
                    var gen = t.GetGenericArguments()[0];
                    if (IsPrimitiveLike(gen))
                    {
                        var en = (System.Collections.IEnumerable)val;
                        var items = new List<string>();
                        int i = 0;
                        foreach (var item in en)
                        {
                            if (i++ >= 64) { items.Add("..."); break; }
                            if (!TryFormatSupportedValue(item, out var itemStr)) return false;
                            items.Add(itemStr);
                        }
                        s = "[" + string.Join(",", items) + "]";
                        return true;
                    }
                }
                return false;
        }
    }

    static bool IsPrimitiveLike(Type t)
    {
        if (t == typeof(string) || t.IsPrimitive) return true;
        if (t.IsEnum) return true;
        return t == typeof(float) || t == typeof(double) || t == typeof(decimal)
            || t == typeof(Vector2) || t == typeof(Vector3) || t == typeof(Vector4)
            || t == typeof(Color) || t == typeof(bool);
    }

    // ===== JSON APURI =====
    static string ToPrettyJson(object obj)
    {
        var raw = JsonUtility.ToJson(obj, false);
        return Pretty(raw);
    }

    static string Pretty(string json)
    {
        var sb = new StringBuilder();
        bool quotes = false;
        int indent = 0;
        for (int i = 0; i < json.Length; i++)
        {
            char ch = json[i];
            switch (ch)
            {
                case '{':
                case '[':
                    sb.Append(ch);
                    if (!quotes)
                    {
                        sb.Append('\n');
                        indent++;
                        sb.Append(new string(' ', indent * 2));
                    }
                    break;
                case '}':
                case ']':
                    if (!quotes)
                    {
                        sb.Append('\n');
                        indent = Math.Max(0, indent - 1);
                        sb.Append(new string(' ', indent * 2));
                        sb.Append(ch);
                    }
                    else sb.Append(ch);
                    break;
                case ',':
                    sb.Append(ch);
                    if (!quotes)
                    {
                        sb.Append('\n');
                        sb.Append(new string(' ', indent * 2));
                    }
                    break;
                case ':':
                    sb.Append(ch);
                    if (!quotes) sb.Append(' ');
                    break;
                case '"':
                    sb.Append(ch);
                    bool escaped = false;
                    int j = i;
                    while (j > 0 && json[--j] == '\\') escaped = !escaped;
                    if (!escaped) quotes = !quotes;
                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }
        return sb.ToString();
    }

    // ===== DTO:t =====
    [Serializable]
    class ProjectContext
    {
        public string unityVersion;
        public string generatedAt;
        public List<SceneContext> scenes;

        // Vanha: pelkät polut
        public List<string> scriptablePaths;

        // Uusi: referoidut SO:t tiivistettynä
        public List<ScriptableObjectInfo> scriptables;

        // Sceneissä käytettyjen komponenttiluokkien koodit (leikatusti)
        public List<ScriptInfo> scripts;
    }

    [Serializable]
    class SceneContext
    {
        public string name;
        public string path;
        public List<GameObjectContext> roots;
    }

    [Serializable]
    class GameObjectContext
    {
        public string name;
        public string path;   // "Parent/Child/.../Obj"
        public string tag;
        public string layer;
        public bool active;
        public string prefab; // None | PrefabAsset | PrefabInstance | PrefabInstance(Missing)
        public List<string> components;
        public Vector3 localPosition;
        public Vector3 localRotation;
        public Vector3 localScale;
        public List<GameObjectContext> children;
    }

    [Serializable]
    class ScriptInfo
    {
        public string name;   // Type.Name
        public string path;   // Asset-polku
        public string code;   // leikattu lähdekoodi
    }

    [Serializable]
    class ScriptableObjectInfo
    {
        public string type;           // SO:n tyyppinimi
        public string name;           // assetin nimi
        public string path;           // AssetDatabase-polku
        public List<KV> fields;       // vain turvalliset skalaari/vektorikentät stringeinä
    }

    [Serializable]
    class KV { public string key; public string value; }
}
#endif
