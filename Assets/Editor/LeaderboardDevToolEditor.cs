using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(LeaderboardDevTool))]
public class LeaderboardDevToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LeaderboardDevTool tool = (LeaderboardDevTool)target;

        GUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Submit Test Score", GUILayout.Height(24)))
        {
            tool.SubmitTestScoreButton();
        }

        if (GUILayout.Button(tool != null && tool.showDebugGUI ? "Hide Dev Window" : "Show Dev Window", GUILayout.Height(24)))
        {
            tool.ToggleDebugGUI();
            EditorUtility.SetDirty(tool);
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif
