using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// 로컬 테스트용 디버그 도구
/// PlayerPrefs에 저장된 데이터(BestScore, 닉네임 등)를 쉽게 수정할 수 있음
/// </summary>
public class LocalDataDebugger : MonoBehaviour
{
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private Button toggleDebugButton;
    
    // BestScore 관련
    [SerializeField] private TMP_InputField bestScoreInputField;
    [SerializeField] private Button setBestScoreButton;
    [SerializeField] private TextMeshProUGUI bestScoreDisplayText;
    
    // 닉네임 관련
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private Button setNicknameButton;
    [SerializeField] private TextMeshProUGUI nicknameDisplayText;
    
    // 플레이어 ID 관련
    [SerializeField] private TextMeshProUGUI playerIdDisplayText;
    
    // 테스트 데이터 버튼들
    [SerializeField] private Button resetAllButton;
    [SerializeField] private Button setTestDataButton;
    [SerializeField] private Button refreshDataButton;
    
    // 추가 정보
    [SerializeField] private TextMeshProUGUI additionalInfoText;
    
    private const string BEST_SCORE_KEY = "BestScore";
    private const string NICKNAME_KEY = "DebugNickname";

    [SerializeField] private KeyCode toggleDebugKeyCode = KeyCode.F12;
    private bool debugEnabled = false;

    private void Start()
    {
        #if UNITY_EDITOR || DEBUG
            debugEnabled = true;
            InitializeUI();
        #else
            // 빌드에서는 패널 비활성화
            if (debugPanel != null)
                debugPanel.SetActive(false);
            debugEnabled = false;
        #endif
    }

    private void Update()
    {
        #if UNITY_EDITOR || DEBUG
            // 단축키로 디버그 패널 토글
            if (debugEnabled && Input.GetKeyDown(toggleDebugKeyCode))
            {
                ToggleDebugPanel();
            }
        #endif
    }

    private void InitializeUI()
    {
        if (debugPanel == null)
        {
            Debug.LogWarning("Debug panel is not assigned");
            return;
        }

        debugPanel.SetActive(false);

        // 토글 버튼
        if (toggleDebugButton != null)
            toggleDebugButton.onClick.AddListener(ToggleDebugPanel);

        // BestScore 버튼
        if (setBestScoreButton != null)
            setBestScoreButton.onClick.AddListener(SetBestScore);

        // 닉네임 버튼
        if (setNicknameButton != null)
            setNicknameButton.onClick.AddListener(SetNickname);

        // 테스트 데이터 버튼
        if (resetAllButton != null)
            resetAllButton.onClick.AddListener(ResetAllData);

        if (setTestDataButton != null)
            setTestDataButton.onClick.AddListener(SetTestData);

        if (refreshDataButton != null)
            refreshDataButton.onClick.AddListener(RefreshDisplay);

        // 초기 데이터 표시
        RefreshDisplay();
    }

    private void ToggleDebugPanel()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
            if (debugPanel.activeSelf)
            {
                RefreshDisplay();
            }
        }
    }

    private void SetBestScore()
    {
        if (bestScoreInputField == null)
        {
            Debug.LogWarning("BestScore input field is not assigned");
            return;
        }

        string input = bestScoreInputField.text;
        if (int.TryParse(input, out int score))
        {
            if (score < 0)
            {
                Debug.LogWarning("Score cannot be negative");
                return;
            }

            PlayerPrefs.SetInt(BEST_SCORE_KEY, score);
            PlayerPrefs.Save();
            Debug.Log($"BestScore set to: {score}");
            RefreshDisplay();
        }
        else
        {
            Debug.LogWarning("Invalid score input: " + input);
        }
    }

    private void SetNickname()
    {
        if (nicknameInputField == null)
        {
            Debug.LogWarning("Nickname input field is not assigned");
            return;
        }

        string nickname = nicknameInputField.text;
        if (string.IsNullOrEmpty(nickname))
        {
            Debug.LogWarning("Nickname cannot be empty");
            return;
        }

        PlayerPrefs.SetString(NICKNAME_KEY, nickname);
        PlayerPrefs.Save();
        Debug.Log($"Nickname set to: {nickname}");
        RefreshDisplay();
    }

    private void ResetAllData()
    {
        #if UNITY_EDITOR
            if (UnityEditor.EditorUtility.DisplayDialog("Reset All Data", 
                "Are you sure you want to reset all local data (BestScore, Nickname)?", 
                "Yes", "No"))
            {
                PlayerPrefs.DeleteKey(BEST_SCORE_KEY);
                PlayerPrefs.DeleteKey(NICKNAME_KEY);
                PlayerPrefs.Save();

                bestScoreInputField.text = "0";
                nicknameInputField.text = "";

                Debug.Log("All local data has been reset");
                RefreshDisplay();
            }
        #else
            // 런타임에서는 확인 없이 바로 초기화
            PlayerPrefs.DeleteKey(BEST_SCORE_KEY);
            PlayerPrefs.DeleteKey(NICKNAME_KEY);
            PlayerPrefs.Save();

            bestScoreInputField.text = "0";
            nicknameInputField.text = "";

            Debug.Log("All local data has been reset");
            RefreshDisplay();
        #endif
    }

    private void SetTestData()
    {
        // 테스트용 샘플 데이터 설정
        PlayerPrefs.SetInt(BEST_SCORE_KEY, 50000);
        PlayerPrefs.SetString(NICKNAME_KEY, "TestPlayer");
        PlayerPrefs.Save();

        Debug.Log("Test data set - BestScore: 50000, Nickname: TestPlayer");
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        // BestScore 표시 및 입력 필드 업데이트
        int currentBestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        if (bestScoreDisplayText != null)
            bestScoreDisplayText.text = $"Current BestScore: {currentBestScore}";
        if (bestScoreInputField != null)
            bestScoreInputField.text = currentBestScore.ToString();

        // 닉네임 표시 및 입력 필드 업데이트
        string currentNickname = PlayerPrefs.GetString(NICKNAME_KEY, "No Nickname");
        if (nicknameDisplayText != null)
            nicknameDisplayText.text = $"Current Nickname: {currentNickname}";
        if (nicknameInputField != null)
            nicknameInputField.text = currentNickname == "No Nickname" ? "" : currentNickname;

        // 플레이어 ID 표시
        try
        {
            string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
            if (playerIdDisplayText != null)
                playerIdDisplayText.text = $"Player ID: {playerId}";
        }
        catch
        {
            if (playerIdDisplayText != null)
                playerIdDisplayText.text = "Player ID: Not initialized";
        }

        // 추가 정보
        if (additionalInfoText != null)
        {
            string additionalInfo = $"PlayerPrefs Keys:\n";
            additionalInfo += $"- BestScore: {currentBestScore}\n";
            additionalInfo += $"- Nickname: {currentNickname}\n";
            additionalInfo += $"\nTotal keys in PlayerPrefs: {GetPlayerPrefsCount()}";
            additionalInfoText.text = additionalInfo;
        }
    }

    // PlayerPrefs의 모든 키를 얻기 (에디터 전용)
    private int GetPlayerPrefsCount()
    {
        #if UNITY_EDITOR
            return UnityEditor.EditorPrefs.GetInt("PlayerPrefsCount", 0);
        #else
            return -1;
        #endif
    }

    // 디버그 패널을 프로그래밍 방식으로 토글 (런타임 테스트용)
    public void ToggleDebug()
    {
        ToggleDebugPanel();
    }

    // 특정 PlayerPrefs 값을 설정하는 공용 메서드
    public void SetPlayerPrefsInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
        Debug.Log($"Set {key} to {value}");
        RefreshDisplay();
    }

    public void SetPlayerPrefsString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
        Debug.Log($"Set {key} to {value}");
        RefreshDisplay();
    }

    public int GetPlayerPrefsInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public string GetPlayerPrefsString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }
}
