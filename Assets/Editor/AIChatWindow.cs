#if UNITY_EDITOR
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AIChatWindow : EditorWindow
{
    const string ContextPath = "Temp/ai_context.json";
    const string PrefKey = "OPENAI_API_KEY";
    const int MaxContextChars = 120_000;

    string apiKey;
    string userPrompt = "";
    Vector2 scroll;
    string lastResponse = "(tyhj‰)";

    [MenuItem("AI/Chat (with Scene Context)")]
    public static void ShowWindow()
    {
        var w = GetWindow<AIChatWindow>("AI Chat");
        w.minSize = new Vector2(480, 360);
        w.Show();
    }

    void OnEnable()
    {
        // Lue API-avain EditorPrefseist‰ tai ymp‰ristˆmuuttujasta
        apiKey = EditorPrefs.GetString(PrefKey, Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("API Key", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        apiKey = EditorGUILayout.PasswordField(apiKey);
        if (GUILayout.Button("Save", GUILayout.Width(80)))
        {
            EditorPrefs.SetString(PrefKey, apiKey);
            ShowNotification(new GUIContent("Saved API key"));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Your prompt", EditorStyles.boldLabel);
        userPrompt = EditorGUILayout.TextArea(userPrompt, GUILayout.MinHeight(80));

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Ask"))
            _ = AskAsync(userPrompt);
        if (GUILayout.Button("Ask + Selection"))
            _ = AskWithSelectionAsync(userPrompt);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Response", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.TextArea(lastResponse, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    async Task AskWithSelectionAsync(string prompt)
    {
        string selectionHint = "";
        if (Selection.activeGameObject != null)
        {
            selectionHint = $"Selected object path: {GetPath(Selection.activeGameObject)}";
        }
        await AskAsync(selectionHint.Length > 0 ? prompt + "\n\n" + selectionHint : prompt);
    }

    async Task AskAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            lastResponse = "API key puuttuu. Tallenna se ikkunan yl‰reunasta.";
            Repaint(); return;
        }

        // Lue snapshot-konteksti
        string context = File.Exists(ContextPath) ? File.ReadAllText(ContextPath) : "{}";
        if (context.Length > MaxContextChars)
            context = context.Substring(0, MaxContextChars) + "\n/* ...truncated... */";

        string system =
@"You are an assistant helping with a Unity project.
You are given a snapshot of currently open scenes and hierarchy as JSON created by an editor tool.
Paths use ""Parent/Child/.../Object"" and are unique within a scene.
Only answer based on the snapshot unless the user explicitly asks for general advice.
If the user refers to an object, they mean 'path' in the JSON. Be concise.";

        string fullUser =
$@"--- SNAPSHOT JSON BEGIN ---
{context}
--- SNAPSHOT JSON END ---

User question:
{prompt}";

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // Rakennetaan runko POCO-luokilla, ei string-kollaaseja
            var req = new ChatRequest
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new Message { role = "system", content = system },
                    new Message { role = "user",   content = fullUser }
                }
            };

            var jsonBody = JsonUtility.ToJson(req);
            // Debug.Log(jsonBody); // halutessasi tarkista l‰hetett‰v‰ runko

            var resp = await client.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(jsonBody, Encoding.UTF8, "application/json")
            );

            var json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                lastResponse = $"HTTP {resp.StatusCode}\n{json}";
            }
            else
            {
                // Yritet‰‰n normaali deserialisointi vastauksesta
                var parsed = JsonUtility.FromJson<ChatResponse>(json);
                if (parsed != null && parsed.choices != null && parsed.choices.Length > 0 &&
                    parsed.choices[0].message != null && !string.IsNullOrEmpty(parsed.choices[0].message.content))
                {
                    lastResponse = parsed.choices[0].message.content;
                }
                else
                {
                    // Jos JsonUtility kompuroi, kaivetaan content h‰t‰varalla
                    lastResponse = ExtractContentFallback(json);
                }
            }
        }
        catch (Exception e)
        {
            lastResponse = e.ToString();
        }

        Repaint();
    }

    static string GetPath(GameObject go)
    {
        var path = go.name;
        var t = go.transform;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    // H‰t‰vara jos JsonUtility ei onnistu vastausrakenteen kanssa
    static string ExtractContentFallback(string json)
    {
        var key = "\"content\":\"";
        int i = json.IndexOf(key, StringComparison.Ordinal);
        if (i < 0) return json;
        i += key.Length;
        var sb = new StringBuilder();
        bool escape = false;
        for (int p = i; p < json.Length; p++)
        {
            char c = json[p];
            if (escape)
            {
                if (c == 'n') sb.Append('\n');
                else if (c == 't') sb.Append('\t');
                else if (c == 'r') sb.Append('\r');
                else if (c == '\\' || c == '"' || c == '/') sb.Append(c);
                else sb.Append(c);
                escape = false;
            }
            else
            {
                if (c == '\\') escape = true;
                else if (c == '"') break;
                else sb.Append(c);
            }
        }
        return sb.ToString();
    }

    // ==== DTO:t pyyntˆˆn ja vastaukseen ====

    [Serializable]
    class ChatRequest
    {
        public string model;
        public Message[] messages;
    }

    [Serializable]
    class Message
    {
        public string role;
        public string content;
    }

    [Serializable]
    class ChatResponse
    {
        public Choice[] choices;
    }

    [Serializable]
    class Choice
    {
        public Message message;
    }
}
#endif
