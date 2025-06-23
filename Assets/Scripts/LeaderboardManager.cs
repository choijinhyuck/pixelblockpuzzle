using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json; // 추가
using UnityEngine.UI; // UI 사용을 위한 네임스페이스 추가
using System.IO; // 추가
using TMPro; // 추가
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LeaderboardManager : MonoBehaviour
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string PlayerId { get; set; }
        public int Score { get; set; }
        public string Nickname { get; set; }
        // UpdateTime 관련 프로퍼티 삭제
    }

    public string leaderboardId = "YOUR_LEADERBOARD_ID";

    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Transform myEntryContainer;
    [SerializeField] private GameObject nicknameInputPanel; // nickname 입력용 패널
    [SerializeField] private TMP_InputField nicknameInputField;   // nickname 입력 필드
    [SerializeField] private Button submitNicknameButton;      // 제출 버튼
    [SerializeField] private TMP_Text nicknameFeedbackText; // 피드백 메시지 UI

    // 캐싱 관련 변수: 마지막으로 성공적으로 데이터를 받아온 시간
    private float lastLeaderboardFetchTime = -5f;
    // 중복 GetLeaderboard 호출 방지 플래그
    private bool isFetchingLeaderboard = false;

    // 금지어 캐싱 (BadWord.txt 파일 내용)
    private static string[] bannedWords;

    void Awake()
    {
        // 금지어 목록 로드 (Assets/TextFilter/BadWord.txt)
        if (bannedWords == null)
        {
            string path = Path.Combine(Application.dataPath, "TextFilter", "BadWord.txt");
            if (File.Exists(path))
            {
                bannedWords = File.ReadAllLines(path);
            }
            else
            {
                Debug.LogWarning("BadWord.txt not found at: " + path);
                bannedWords = new string[0];
            }
        }
    }

    async void Start()
    {
        await InitializeUnityServicesAsync();
        
        // 게임 실행 시 nickname panel이 활성화되어 있다면 닫음
        if (nicknameInputPanel != null && nicknameInputPanel.activeSelf)
        {
            submitNicknameButton.interactable = false;
            nicknameInputPanel.SetActive(false);
        }
    }

    async Task InitializeUnityServicesAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Debug.Log("Unity Services and Authentication initialized.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to initialize Unity Services: " + ex.Message);
        }
    }

    // nickname을 인자로 받아 metadata 포함해서 스코어를 제출
    public async void SubmitScore(int score)
    {
        bool needNickname = false;
        string existingNickname = null;
        string nickname = null;

        // 먼저 플레이어의 현재 점수 기록 조회 시도
        try
        {
            var myScoreResponse = await LeaderboardsService.Instance.GetPlayerScoreAsync(
                leaderboardId, new GetPlayerScoreOptions { IncludeMetadata = true }
            );

            if (myScoreResponse != null)
            {
                Dictionary<string, string> metadataDict = null;
                try
                {
                    metadataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(myScoreResponse.Metadata);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Failed to parse metadata: " + ex.Message);
                }

                if (metadataDict == null || !metadataDict.TryGetValue("nickname", out existingNickname) || string.IsNullOrEmpty(existingNickname))
                {
                    needNickname = true;
                }
            }
            else
            {
                needNickname = true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to retrieve player score: " + ex.Message);
            needNickname = true;
        }

        // nickname 정보가 없으면 UI를 통해 입력 요청
        if (needNickname && string.IsNullOrEmpty(nickname))
        {
            Debug.LogWarning("No nickname available. Prompting for nickname.");
            PromptForNickname(async (enteredNickname) => {
                // 입력 받은 nickname을 사용하여 다시 SubmitScore 호출
                await SubmitScoreWithNickname(score, enteredNickname);
            });
            return;
        }
        else if (!needNickname && string.IsNullOrEmpty(nickname))
        {
            // 기존 기록에 nickname이 있을 경우
            nickname = existingNickname;
        }

        // nickname이 있으면 바로 점수 제출
        await SubmitScoreWithNickname(score, nickname);
    }

    // 실제 점수 제출을 처리하는 보조 함수 (업데이트 시간 관련 처리 삭제)
    private async Task SubmitScoreWithNickname(int score, string nickname)
    {
        try
        {
            Dictionary<string, string> metadataData = new Dictionary<string, string>
            {
                { "nickname", nickname },
            };

            var options = new AddPlayerScoreOptions
            {
                Metadata = metadataData
            };

            // 최초 점수 제출 (업데이트 시간 관련 로직 삭제)
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score, options);
            Debug.Log("Score submitted: " + score + " with nickname: " + nickname);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to submit score: " + ex.Message);
        }
    }

    // 기존의 IsNicknameValid는 간단히 ValidateNickname 호출하도록 수정.
    private bool IsNicknameValid(string nickname)
    {
        return ValidateNickname(nickname) == null;
    }

    // 유효하지 않은 경우 이유 메시지 리턴, 유효하면 null 리턴
    private string ValidateNickname(string nickname)
    {
        // 1. 기본 허용 문자 체크 (영문, 숫자, 밑줄, 공백, 한글)
        string pattern = @"^[a-zA-Z0-9_ \uAC00-\uD7A3]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(nickname, pattern))
        {
            // Localization string table "UI", key "NicknameInvalidCharacters"
            return LocalizationSettings.StringDatabase.GetLocalizedString("UI", "NicknameInvalidCharacters");
        }

        // 2. 금지어 필터 (대소문자 구분 없이 포함 여부 검사)
        foreach (string bad in bannedWords)
        {
            if (string.IsNullOrWhiteSpace(bad))
                continue;
            if (nickname.IndexOf(bad, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Localization string table "UI", key "NicknameContainsBannedWord"
                // 해당 키는 포맷 문자열로, 예: "Nickname contains banned word: {0}" 혹은 한글 번역
                string message = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "NicknameContainsBannedWord");
                return string.Format(message, bad);
            }
        }

        // 3. 총 바이트 수 체크 (UTF-8 기준 20바이트 초과 불가)
        int byteCount = System.Text.Encoding.UTF8.GetByteCount(nickname);
        if (byteCount > 20)
        {
            // Localization string table "UI", key "NicknameExceedsMaxBytes"
            return LocalizationSettings.StringDatabase.GetLocalizedString("UI", "NicknameTooLong");
        }

        return null;
    }

    // inputField 내용 변경 시 호출되어 실시간으로 검증 수행
    private void OnNicknameInputChanged(string nickname)
    {
        if (string.IsNullOrEmpty(nickname))
        {
            if (nicknameFeedbackText != null)
            {
                // Localization string table "UI", key "NicknameEmpty"
                string localizedMessage = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "NicknameEmpty");
                nicknameFeedbackText.text = localizedMessage;
                nicknameFeedbackText.color = Color.red;
            }
            submitNicknameButton.interactable = false;
            return;
        }

        string validationMsg = ValidateNickname(nickname);
        if (validationMsg != null)
        {
            if (nicknameFeedbackText != null)
            {
                // Localization string table "UI", key "NicknameInvalid"
                string localizedValidation = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "NicknameInvalid");
                // 필요에 따라 validationMsg 포함 메시지 처리 가능
                nicknameFeedbackText.text = $"{localizedValidation}: {validationMsg}";
                nicknameFeedbackText.color = Color.red;
            }
            submitNicknameButton.interactable = false;
        }
        else
        {
            if (nicknameFeedbackText != null)
            {
                // Localization string table "UI", key "NicknameValid"
                string localizedValid = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "NicknameValid");
                nicknameFeedbackText.text = localizedValid;
                nicknameFeedbackText.color = new Color(0, 0.6f, 0); // green 계열
            }
            submitNicknameButton.interactable = true;
        }
    }

    // nickname이 없을 경우 호출할 UI 입력 함수. onNicknameSubmitted 콜백을 넘겨 입력 후 처리.
    public void PromptForNickname(System.Action<string> onNicknameSubmitted)
    {
        GameManager.Instance.uiManager.OnClickPanelToggleButton(nicknameInputPanel);
        // panel 열릴 때 초기화
        nicknameInputField.text = string.Empty;
        if (nicknameFeedbackText != null)
        {
            // 바로 빈 문자열에 대해 검증하여 "NicknameEmpty" 피드백 표시
            OnNicknameInputChanged(string.Empty);
        }
        submitNicknameButton.interactable = false;

        // inputField 변경 이벤트: 값을 변경할 때마다 검증하도록 설정
        nicknameInputField.onValueChanged.RemoveAllListeners();
        nicknameInputField.onValueChanged.AddListener(OnNicknameInputChanged);

        // submit 버튼 클릭 이벤트 등록
        submitNicknameButton.onClick.RemoveAllListeners();
        submitNicknameButton.onClick.AddListener(() =>
        {
            string enteredNickname = nicknameInputField.text;
            if (!string.IsNullOrEmpty(enteredNickname))
            {
                if (!IsNicknameValid(enteredNickname))
                {
                    // 유효하지 않은 경우 onValueChanged에서 피드백 표시됨
                    return;
                }
                // panel 닫을 때 submit 버튼 상태 초기화
                GameManager.Instance.uiManager.OnClickPanelCloseButton(nicknameInputPanel);
                submitNicknameButton.interactable = false;
                onNicknameSubmitted.Invoke(enteredNickname);
            }
            else
            {
                Debug.LogWarning("Nickname is empty. Please enter a valid nickname.");
            }
        });
    }

    public async void GetLeaderboard()
    {
        if (Time.time - lastLeaderboardFetchTime < 5f)
        {
            Debug.Log(Time.time);
            Debug.Log(lastLeaderboardFetchTime);
            Debug.Log("GetLeaderboard called too soon. Returning cached leaderboard.");
            return;
        }

        if (isFetchingLeaderboard) return;
        isFetchingLeaderboard = true;
        lastLeaderboardFetchTime = Time.time;

        try
        {
            var leaderboardResponse = await LeaderboardsService.Instance.GetScoresAsync(
                leaderboardId, new GetScoresOptions { Limit = 100, IncludeMetadata = true }
            );

            string playerId = AuthenticationService.Instance.PlayerId;
            LeaderboardEntry myEntry = null;

            int resultCount = leaderboardResponse.Results.Count;
            int childCount = leaderboardContent.childCount;

            for (int i = 0; i < resultCount; i++)
            {
                var entry = leaderboardResponse.Results[i];
                LeaderboardEntryUI entryUI = null;
                if (i < childCount)
                {
                    entryUI = leaderboardContent.GetChild(i).GetComponent<LeaderboardEntryUI>();
                    leaderboardContent.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    GameObject newEntry = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                    entryUI = newEntry.GetComponent<LeaderboardEntryUI>();
                }

                if (entryUI != null)
                {
                    Dictionary<string, string> metadataDict = null;
                    try
                    {
                        metadataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(entry.Metadata);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to parse metadata: " + ex.Message);
                    }
                    string nickname = "Unknown";
                    if (metadataDict != null && metadataDict.TryGetValue("nickname", out string name))
                        nickname = name;

                    // UpdateTime, scroedate 관련 로직 삭제 → UI에 점수와 닉네임만 전달
                    entryUI.SetData(entry.Rank + 1, nickname, (int)entry.Score);
                }

                if (entry.PlayerId == playerId)
                {
                    Dictionary<string, string> metadataDict = null;
                    try
                    {
                        metadataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(entry.Metadata);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to parse metadata for my entry: " + ex.Message);
                    }
                    string nickname = "Unknown";
                    if (metadataDict != null && metadataDict.TryGetValue("nickname", out string name))
                        nickname = name;

                    myEntry = new LeaderboardEntry
                    {
                        Rank = entry.Rank,
                        PlayerId = entry.PlayerId,
                        Score = (int)entry.Score,
                        Nickname = nickname
                    };
                }
            }

            for (int i = resultCount; i < childCount; i++)
            {
                leaderboardContent.GetChild(i).gameObject.SetActive(false);
            }

            if (myEntry == null)
            {
                try
                {
                    var myScoreResponse = await LeaderboardsService.Instance.GetPlayerScoreAsync(
                        leaderboardId, new GetPlayerScoreOptions { IncludeMetadata = true }
                    );
                    if (myScoreResponse != null)
                    {
                        Dictionary<string, string> metadataDict = null;
                        try
                        {
                            metadataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(myScoreResponse.Metadata);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Failed to parse metadata for my entry: " + ex.Message);
                        }
                        string nickname = "Unknown";
                        if (metadataDict != null && metadataDict.TryGetValue("nickname", out string name))
                            nickname = name;

                        myEntry = new LeaderboardEntry
                        {
                            Rank = myScoreResponse.Rank,
                            PlayerId = myScoreResponse.PlayerId,
                            Score = (int)myScoreResponse.Score,
                            Nickname = nickname
                        };
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Failed to retrieve my score: " + ex.Message);
                }
            }

            if (myEntry != null)
            {
                if (myEntryContainer != null)
                {
                    LeaderboardEntryUI myEntryUI = myEntryContainer.GetComponent<LeaderboardEntryUI>();
                    if (myEntryUI != null)
                    {
                        myEntryUI.SetData(myEntry.Rank + 1, myEntry.Nickname, myEntry.Score);
                        myEntryContainer.gameObject.SetActive(true);
                    }
                }
                else
                {
                    Debug.LogWarning("MyEntry UI is missing in the scene. Please ensure it is pre-populated in myEntryContainer.");
                }
            }
            else
            {
                if (myEntryContainer != null)
                {
                    LeaderboardEntryUI myEntryUI = myEntryContainer.GetComponent<LeaderboardEntryUI>();
                    if (myEntryUI != null)
                    {
                        myEntryUI.Reset();
                        myEntryContainer.gameObject.SetActive(true);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to retrieve leaderboard: " + ex.Message);
        }
        finally
        {
            isFetchingLeaderboard = false;
        }
    }

    // 테스트용: 100개의 무작위 데이터로 리더보드 시뮬레이션
    public void SimulateRandomLeaderboardEntries()
    {
        List<LeaderboardEntry> simulatedEntries = new List<LeaderboardEntry>();
        for (int i = 0; i < 100; i++)
        {
            string randomPlayerId = "Player" + UnityEngine.Random.Range(1000, 10000);
            int randomScore = UnityEngine.Random.Range(0, 10000);
            simulatedEntries.Add(new LeaderboardEntry
            {
                Rank = i,
                PlayerId = randomPlayerId,
                Score = randomScore,
                Nickname = "TestUser" + randomPlayerId
                // UpdateTime 관련 프로퍼티 제거
            });
        }

        simulatedEntries = simulatedEntries.OrderByDescending(entry => entry.Score).ToList();
        for (int i = 0; i < simulatedEntries.Count; i++)
        {
            simulatedEntries[i].Rank = i;
        }

        int entryCount = simulatedEntries.Count;
        int childCount = leaderboardContent.childCount;

        for (int i = 0; i < entryCount; i++)
        {
            LeaderboardEntry simulatedEntry = simulatedEntries[i];
            LeaderboardEntryUI entryUI = null;
            if (i < childCount)
            {
                entryUI = leaderboardContent.GetChild(i).GetComponent<LeaderboardEntryUI>();
                leaderboardContent.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                GameObject newEntry = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                entryUI = newEntry.GetComponent<LeaderboardEntryUI>();
            }

            if (entryUI != null)
            {
                entryUI.SetData(simulatedEntry.Rank + 1, simulatedEntry.Nickname, simulatedEntry.Score);
            }
        }

        for (int i = entryCount; i < childCount; i++)
        {
            leaderboardContent.GetChild(i).gameObject.SetActive(false);
        }
    }
}