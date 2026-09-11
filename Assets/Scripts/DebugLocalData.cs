using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Threading.Tasks;
using DG.Tweening;

/// <summary>
/// Inspector에서 직접 로컬 데이터를 수정하는 디버거
/// Apply 버튼: 값 저장 및 즉시 UI 업데이트
/// Submit 버튼: 서버 리더보드와 비교후 필요시 갱신
/// </summary>
public class DebugLocalData : MonoBehaviour
{
    [Header("--- BEST SCORE ---")]
    public int bestScore = 0;

    [Header("--- NICKNAME ---")]
    [TextArea(1, 3)]
    public string nickname = "";

    [Header("--- PLAYER ID (읽기 전용) ---")]
    [SerializeField] private string playerId = "";

    [Header("--- 서버 정보 (읽기 전용) ---")]
    [SerializeField] private int serverBestScore = 0;
    [SerializeField] private bool isCheckingServer = false;

    private void Start()
    {
        // 게임 시작 시 한번만 로드
        #if UNITY_EDITOR
            LoadFromPlayerPrefs();
        #endif
    }

    public void LoadFromPlayerPrefs()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        nickname = PlayerPrefs.GetString("DebugNickname", "");
        
        try
        {
            playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
        }
        catch
        {
            playerId = "Not initialized";
        }
    }

    public void ApplyChanges()
    {
        // PlayerPrefs에 저장
        PlayerPrefs.SetInt("BestScore", bestScore);
        PlayerPrefs.SetString("DebugNickname", nickname);
        PlayerPrefs.Save();
        
        // UI 즉시 업데이트
        UpdateUI();
        
        Debug.Log($"✓ 적용됨:\n- BestScore: {bestScore}\n- Nickname: {nickname}\n- UI 업데이트 완료");
    }

    private void UpdateUI()
    {
        // ScoreManager가 있으면 메모리와 UI 모두 업데이트
        if (GameManager.Instance != null && GameManager.Instance.scoreManager != null)
        {
            ScoreManager scoreManager = GameManager.Instance.scoreManager;
            
            // 중요: ScoreManager의 메모리 bestScore도 동기화해야 함
            // (그렇지 않으면 게임오버 시 PlayerPrefs가 덮어써짐)
            scoreManager.SetBestScore(bestScore);
            
            Debug.Log("✓ UI 및 메모리 업데이트 완료 (PlayerPrefs와 메모리 동기화)");
        }
        else
        {
            Debug.LogWarning("⚠ GameManager 또는 ScoreManager를 찾을 수 없습니다");
        }
    }

    public async void SubmitToServer()
    {
        if (isCheckingServer)
        {
            Debug.LogWarning("⚠ 이미 서버와 동기화 작업 중입니다");
            return;
        }

        isCheckingServer = true;
        Debug.Log("🔄 서버와 동기화 시작...");

        try
        {
            // 게임 오버 상황과 동일한 로직 실행
            if (GameManager.Instance != null && GameManager.Instance.leaderboardManager != null)
            {
                await GameManager.Instance.leaderboardManager.SyncLocalBestWithServerAfterGameEnd();
                Debug.Log("✓ 서버 동기화 완료");
            }
            else
            {
                Debug.LogError("❌ LeaderboardManager를 찾을 수 없습니다");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ 서버 동기화 실패: {ex.Message}");
        }
        finally
        {
            isCheckingServer = false;
        }
    }

    public void ResetAll()
    {
        #if UNITY_EDITOR
            if (UnityEditor.EditorUtility.DisplayDialog("초기화 확인",
                "모든 로컬 데이터를 초기화하시겠습니까?\n(BestScore, Nickname)",
                "Yes", "No"))
            {
                PlayerPrefs.DeleteKey("BestScore");
                PlayerPrefs.DeleteKey("DebugNickname");
                PlayerPrefs.Save();
                
                bestScore = 0;
                nickname = "";
                
                Debug.Log("✓ 모든 로컬 데이터가 초기화되었습니다");
            }
        #endif
    }

    public void SetTestData()
    {
        bestScore = 50000;
        nickname = "TestPlayer";
        ApplyChanges();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DebugLocalData))]
public class DebugLocalDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DebugLocalData debugger = (DebugLocalData)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("--- 액션 ---", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Apply 버튼 (큼, 초록색) - 값 저장 및 UI 업데이트
        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
        if (GUILayout.Button("✓ APPLY (저장 & UI 업데이트)", GUILayout.Height(40)))
        {
            debugger.ApplyChanges();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);

        // Submit 버튼 (큼, 주황색) - 서버 동기화
        GUI.backgroundColor = new Color(1f, 0.6f, 0.0f);
        if (GUILayout.Button("📤 SUBMIT (서버 동기화)", GUILayout.Height(40)))
        {
            debugger.SubmitToServer();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);

        // 2열 레이아웃
        EditorGUILayout.BeginHorizontal();

        // 테스트 데이터 버튼 (파란색)
        GUI.backgroundColor = new Color(0.3f, 0.6f, 1f);
        if (GUILayout.Button("📊 Test Data", GUILayout.Height(35)))
        {
            debugger.SetTestData();
        }

        // 초기화 버튼 (빨간색)
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("🗑 Reset", GUILayout.Height(35)))
        {
            debugger.ResetAll();
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        // 새로고침 버튼 (회색)
        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        if (GUILayout.Button("🔄 Reload", GUILayout.Height(30)))
        {
            debugger.LoadFromPlayerPrefs();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);

        // 정보
        EditorGUILayout.HelpBox(
            "🎮 사용 방법:\n\n" +
            "1. BestScore 값을 입력합니다\n" +
            "2. APPLY 버튼 → 저장 + UI 즉시 업데이트\n" +
            "3. SUBMIT 버튼 → 서버와 동기화\n" +
            "   (로컬이 높으면 서버에 갱신)\n\n" +
            "📁 Test Data: 테스트 데이터 자동 설정\n" +
            "🗑 Reset: 모든 데이터 초기화\n" +
            "🔄 Reload: PlayerPrefs에서 다시 로드"
            , MessageType.Info);
    }
}
#endif
