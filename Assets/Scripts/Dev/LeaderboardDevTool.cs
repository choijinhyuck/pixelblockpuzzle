using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;

// 개발/테스트용: 에디터/빌드에서 런타임으로 특정 리더보드에 테스트 점수를 전송할 수 있는 간단한 툴입니다.
// - Inspector에서 값 설정 후 버튼으로 전송하거나, `autoSubmitOnStart`를 켜면 Start 시 자동 전송합니다.
// - 다른 플레이어의 점수 변경은 관리자 권한(서버)이 필요합니다. 이 스크립트는 현재 로그인된 플레이어로만 제출합니다.
public class LeaderboardDevTool : MonoBehaviour
{
    [Header("Target")]
    public string leaderboardIdOverride = "";
    public int score = 0;
    public string nickname = "DevTester";

    [Header("Behavior")]
    public bool autoSubmitOnStart = false;
    public bool showDebugGUI = true;

    private Rect windowRect = new Rect(10, 10, 340, 180);

    async void Start()
    {
        await InitializeUnityServicesAsync();
        if (autoSubmitOnStart)
        {
            await SubmitTestScoreAsync();
        }
    }

    public async Task InitializeUnityServicesAsync()
    {
        try
        {
            // Initialize Unity Services (multiple calls are safe)
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Debug.Log("LeaderboardDevTool: Unity Services initialized and signed in.");
        }
        catch (Exception ex)
        {
            Debug.LogError("LeaderboardDevTool: Failed to initialize Unity Services: " + ex.Message);
        }
    }

    // Inspector의 버튼이나 다른 코드에서 호출 가능
    public async Task SubmitTestScoreAsync()
    {
        if (string.IsNullOrEmpty(leaderboardIdOverride))
        {
            Debug.LogError("LeaderboardDevTool: leaderboardIdOverride is empty. Set target leaderboard ID.");
            return;
        }

        string nick = string.IsNullOrEmpty(nickname) ? "DevTester" : nickname;
        var metadata = new Dictionary<string, string> { { "nickname", nick } };
        var options = new AddPlayerScoreOptions { Metadata = metadata };

        try
        {
            Debug.Log($"LeaderboardDevTool: Attempting submit. leaderboardId='{leaderboardIdOverride}', score={score}, nickname='{nick}'");
            Debug.Log($"LeaderboardDevTool: Auth signed-in={AuthenticationService.Instance.IsSignedIn}, PlayerId={AuthenticationService.Instance.PlayerId}");
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardIdOverride, score, options);
            Debug.Log($"LeaderboardDevTool: Submitted {score} to {leaderboardIdOverride} as '{nick}'.");

            // 장면 내 LeaderboardManager가 있으면 즉시 갱신 호출
            var lm = FindFirstObjectByType<LeaderboardManager>();
            if (lm != null)
            {
                lm.GetLeaderboard();
            }
        }
        catch (Exception ex)
        {
            if (ex is Unity.Services.Core.RequestFailedException rfe)
            {
                Debug.LogError($"LeaderboardDevTool: RequestFailedException when submitting test score. ErrorCode={rfe.ErrorCode}, Message={rfe.Message}, Details={rfe.StackTrace}");
            }
            else
            {
                Debug.LogError("LeaderboardDevTool: Failed to submit test score: " + ex.ToString());
            }
        }
    }

    [ContextMenu("Submit Test Score (Context Menu)")]
    public void SubmitTestScoreContext()
    {
        _ = SubmitTestScoreAsync();
    }

    // Unity UI 버튼에서 호출할 수 있는 래퍼 (Inspector의 Button OnClick에 연결)
    public void SubmitTestScoreButton()
    {
        _ = SubmitTestScoreAsync();
    }

    // UI에서 디버그 윈도우 보이기/숨기기 토글용
    public void ToggleDebugGUI()
    {
        showDebugGUI = !showDebugGUI;
    }

    void OnGUI()
    {
        if (!showDebugGUI) return;
        windowRect = GUI.Window(123456, windowRect, DevWindow, "Leaderboard Dev Tool");
    }

    void DevWindow(int id)
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Leaderboard ID:");
        leaderboardIdOverride = GUILayout.TextField(leaderboardIdOverride);

        GUILayout.Label("Score:");
        string scoreStr = GUILayout.TextField(score.ToString());
        int.TryParse(scoreStr, out score);

        GUILayout.Label("Nickname:");
        nickname = GUILayout.TextField(nickname);

        GUILayout.Space(8);
        if (GUILayout.Button("Submit Test Score"))
        {
            _ = SubmitTestScoreAsync();
        }

        if (GUILayout.Button("Toggle Auto Submit On Start"))
        {
            autoSubmitOnStart = !autoSubmitOnStart;
        }

        if (GUILayout.Button("Hide/Show Debug GUI"))
        {
            showDebugGUI = !showDebugGUI;
        }

        GUILayout.EndVertical();
        GUI.DragWindow();
    }
}
