using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Text;

// Editor-only admin tool to call Unity Leaderboards Admin REST API locally.
// WARNING: This uses an admin token. Do NOT commit or expose the token.
public class LeaderboardAdminTool : EditorWindow
{
    private string projectId = "";
    private string adminBearerToken = "";
    private string leaderboardId = "";
    private string playerId = "";
    private long score = 0;
    private string metadataJson = "{}";

    private string lastResponse = "";
    private Vector2 scrollPos;

    [MenuItem("Tools/Leaderboard Admin Tool")]
    public static void ShowWindow()
    {
        GetWindow<LeaderboardAdminTool>("Leaderboard Admin");
    }

    void OnGUI()
    {
        GUILayout.Label("Unity Leaderboards Admin (Editor only)", EditorStyles.boldLabel);

        projectId = EditorGUILayout.TextField("Project ID", projectId);
        adminBearerToken = EditorGUILayout.PasswordField("Admin Bearer Token", adminBearerToken);
        leaderboardId = EditorGUILayout.TextField("Leaderboard ID", leaderboardId);
        playerId = EditorGUILayout.TextField("Player ID", playerId);
        score = EditorGUILayout.LongField("Score", score);
        metadataJson = EditorGUILayout.TextField("Metadata (JSON)", metadataJson);

        GUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Update Player Score", GUILayout.Height(30)))
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(adminBearerToken) || string.IsNullOrEmpty(leaderboardId) || string.IsNullOrEmpty(playerId))
            {
                EditorUtility.DisplayDialog("Missing fields", "Please fill Project ID, Admin Bearer Token, Leaderboard ID and Player ID.", "OK");
            }
            else
            {
                UpdatePlayerScore();
            }
        }

        if (GUILayout.Button("Clear Response", GUILayout.Height(30)))
        {
            lastResponse = "";
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);
        GUILayout.Label("Response:");
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        EditorGUILayout.TextArea(lastResponse);
        EditorGUILayout.EndScrollView();
    }

    private void UpdatePlayerScore()
    {
        // Construct URL. If your Admin API endpoint differs, change this.
        string url = $"https://leaderboards.cloud.unity3d.com/v1/projects/{projectId}/leaderboards/{leaderboardId}/players/{playerId}/score";

        var bodyObj = new
        {
            playerId = playerId,
            score = score,
            metadata = TryParseJson(metadataJson)
        };

        string json = JsonUtility.ToJson(new Wrapper(bodyObj));

        var uwr = new UnityWebRequest(url, "PUT");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + adminBearerToken);

        // Synchronous wait (editor only) — short blocking call
        var operation = uwr.SendWebRequest();
        while (!operation.isDone)
        {
            System.Threading.Thread.Sleep(10);
        }

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            lastResponse = $"Status: {uwr.responseCode}\nBody:\n{uwr.downloadHandler.text}";
        }
        else
        {
            lastResponse = $"Error: {uwr.responseCode}\nMessage:\n{uwr.error}\nBody:\n{uwr.downloadHandler.text}";
        }

        Debug.Log("LeaderboardAdminTool response:\n" + lastResponse);
    }

    private object TryParseJson(string json)
    {
        // JsonUtility can't parse arbitrary JSON into object, so return raw string for now.
        // Admin API usually expects metadata object; user can input simple JSON like {"nickname":"X"}.
        try
        {
            return JsonUtility.FromJson<JsonObject>(json);
        }
        catch
        {
            return new {};
        }
    }

    // JsonUtility requires a wrapper class for dynamic content; use simple wrapper
    [System.Serializable]
    private class Wrapper
    {
        public object payload;
        public Wrapper(object o) { payload = o; }
    }

    [System.Serializable]
    private class JsonObject { }
}
