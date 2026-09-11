using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// 간단한 로컬 데이터 디버거
/// 스크립트 추가하면 자동으로 UI 생성 (동적 생성)
/// 따로 설정할 것 없음!
/// </summary>
public class SimpleLocalDataDebugger : MonoBehaviour
{
    private Canvas debugCanvas;
    private bool debugPanelActive = false;
    
    private const string BEST_SCORE_KEY = "BestScore";
    private const string NICKNAME_KEY = "DebugNickname";
    private const string TOGGLE_DEBUG_PREF = "DebugToolVisible";

    void Start()
    {
        #if UNITY_EDITOR || DEBUG
            CreateDebugUI();
        #endif
    }

    void Update()
    {
        #if UNITY_EDITOR || DEBUG
            if (Input.GetKeyDown(KeyCode.F12))
            {
                ToggleDebugPanel();
            }
        #endif
    }

    private void CreateDebugUI()
    {
        // 기존 Canvas 찾기 또는 생성
        debugCanvas = FindObjectOfType<Canvas>();
        
        if (debugCanvas == null)
        {
            GameObject canvasObj = new GameObject("DebugCanvas");
            debugCanvas = canvasObj.AddComponent<Canvas>();
            debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 메인 패널 생성
        GameObject panelObj = new GameObject("DebugPanel");
        panelObj.transform.SetParent(debugCanvas.transform, false);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 닫기 클릭 감지
        Button panelButton = panelObj.AddComponent<Button>();
        panelButton.onClick.AddListener(() => ToggleDebugPanel());

        // 콘텐츠 영역 (클릭 통과 방지)
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(panelObj.transform, false);
        
        Image contentImage = contentObj.AddComponent<Image>();
        contentImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        
        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(400, 500);

        Button contentButton = contentObj.AddComponent<Button>();
        contentButton.onClick.AddListener(() => { }); // 부모 클릭 전파 방지

        // 수직 레이아웃
        VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(15, 15, 15, 15);
        layout.spacing = 10;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        // 제목
        AddTitle(contentObj, "🛠 LOCAL DATA DEBUG");

        // BestScore 섹션
        AddScoreSection(contentObj);

        // 닉네임 섹션
        AddNicknameSection(contentObj);

        // Player ID 표시
        AddPlayerIdDisplay(contentObj);

        // 버튼 그룹
        AddButtonGroup(contentObj);

        debugPanelActive = PlayerPrefs.GetInt(TOGGLE_DEBUG_PREF, 1) == 1;
        panelObj.SetActive(debugPanelActive);
        
        contentObj.SetActive(true);
    }

    private void AddTitle(GameObject parent, string text)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent.transform, false);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = text;
        titleText.fontSize = 28;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.yellow;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(370, 40);
        
        LayoutElement layoutElement = titleObj.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 40;
    }

    private void AddScoreSection(GameObject parent)
    {
        // 라벨
        AddLabel(parent, "BEST SCORE");

        // 입력 필드와 버튼
        GameObject scoreContainerObj = new GameObject("ScoreContainer");
        scoreContainerObj.transform.SetParent(parent.transform, false);
        
        HorizontalLayoutGroup scoreLayout = scoreContainerObj.AddComponent<HorizontalLayoutGroup>();
        scoreLayout.spacing = 8;
        scoreLayout.childForceExpandHeight = false;
        
        RectTransform scoreContainerRect = scoreContainerObj.GetComponent<RectTransform>();
        LayoutElement scoreLayoutElement = scoreContainerObj.AddComponent<LayoutElement>();
        scoreLayoutElement.preferredHeight = 35;

        // 입력 필드
        GameObject inputObj = CreateInputField(scoreContainerObj, "50000", 150);
        TMP_InputField scoreInput = inputObj.GetComponent<TMP_InputField>();

        // 버튼
        GameObject btnObj = CreateButton(scoreContainerObj, "SET", () =>
        {
            if (int.TryParse(scoreInput.text, out int score) && score >= 0)
            {
                PlayerPrefs.SetInt(BEST_SCORE_KEY, score);
                PlayerPrefs.Save();
                Debug.Log($"✓ BestScore set to {score}");
                RefreshAllDisplays();
            }
            else
            {
                Debug.LogWarning("⚠ Invalid score value");
            }
        });
        
        LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
        btnLayout.preferredWidth = 100;

        // 현재값 표시
        int currentScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        AddLabel(parent, $"Current: <color=yellow>{currentScore}</color>");
    }

    private void AddNicknameSection(GameObject parent)
    {
        AddLabel(parent, "NICKNAME");

        GameObject nicknameContainerObj = new GameObject("NicknameContainer");
        nicknameContainerObj.transform.SetParent(parent.transform, false);
        
        HorizontalLayoutGroup nicknameLayout = nicknameContainerObj.AddComponent<HorizontalLayoutGroup>();
        nicknameLayout.spacing = 8;
        nicknameLayout.childForceExpandHeight = false;
        
        LayoutElement nicknameLayoutElement = nicknameContainerObj.AddComponent<LayoutElement>();
        nicknameLayoutElement.preferredHeight = 35;

        string currentNickname = PlayerPrefs.GetString(NICKNAME_KEY, "");
        GameObject nicknameInputObj = CreateInputField(nicknameContainerObj, currentNickname, 200);
        TMP_InputField nicknameInput = nicknameInputObj.GetComponent<TMP_InputField>();

        GameObject nicknameBtnObj = CreateButton(nicknameContainerObj, "SET", () =>
        {
            if (!string.IsNullOrEmpty(nicknameInput.text))
            {
                PlayerPrefs.SetString(NICKNAME_KEY, nicknameInput.text);
                PlayerPrefs.Save();
                Debug.Log($"✓ Nickname set to {nicknameInput.text}");
                RefreshAllDisplays();
            }
        });

        LayoutElement nicknameBtnLayout = nicknameBtnObj.AddComponent<LayoutElement>();
        nicknameBtnLayout.preferredWidth = 100;

        string displayNickname = string.IsNullOrEmpty(currentNickname) ? "None" : currentNickname;
        AddLabel(parent, $"Current: <color=yellow>{displayNickname}</color>");
    }

    private void AddPlayerIdDisplay(GameObject parent)
    {
        AddLabel(parent, "PLAYER ID");
        
        try
        {
            string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
            AddLabel(parent, $"<color=cyan>{playerId}</color>");
        }
        catch
        {
            AddLabel(parent, "<color=red>Not initialized</color>");
        }
    }

    private void AddButtonGroup(GameObject parent)
    {
        // 공간 추가
        AddSpacer(parent, 10);

        // 버튼 컨테이너
        GameObject buttonContainerObj = new GameObject("ButtonContainer");
        buttonContainerObj.transform.SetParent(parent.transform, false);
        
        GridLayoutGroup gridLayout = buttonContainerObj.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(170, 40);
        gridLayout.spacing = new Vector2(8, 8);
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        
        LayoutElement gridLayoutElement = buttonContainerObj.AddComponent<LayoutElement>();
        gridLayoutElement.preferredHeight = 100;

        // 테스트 데이터 버튼
        CreateButton(buttonContainerObj, "📊 TEST DATA", () =>
        {
            PlayerPrefs.SetInt(BEST_SCORE_KEY, 50000);
            PlayerPrefs.SetString(NICKNAME_KEY, "TestPlayer");
            PlayerPrefs.Save();
            Debug.Log("✓ Test data set");
            RefreshAllDisplays();
        });

        // 초기화 버튼
        CreateButton(buttonContainerObj, "🗑 RESET ALL", () =>
        {
            #if UNITY_EDITOR
                if (UnityEditor.EditorUtility.DisplayDialog("Reset All", 
                    "정말로 모든 로컬 데이터를 초기화하시겠습니까?", "Yes", "No"))
                {
                    PlayerPrefs.DeleteKey(BEST_SCORE_KEY);
                    PlayerPrefs.DeleteKey(NICKNAME_KEY);
                    PlayerPrefs.Save();
                    Debug.Log("✓ All data reset");
                    RefreshAllDisplays();
                }
            #else
                PlayerPrefs.DeleteKey(BEST_SCORE_KEY);
                PlayerPrefs.DeleteKey(NICKNAME_KEY);
                PlayerPrefs.Save();
                Debug.Log("✓ All data reset");
                RefreshAllDisplays();
            #endif
        });

        // 동기화 테스트 버튼
        CreateButton(buttonContainerObj, "🔄 SYNC TEST", () =>
        {
            if (GameManager.Instance != null && GameManager.Instance.leaderboardManager != null)
            {
                GameManager.Instance.leaderboardManager.GetLeaderboard();
                Debug.Log("✓ Sync test initiated");
            }
            else
            {
                Debug.LogWarning("⚠ GameManager not found");
            }
        });

        // 종료 버튼
        CreateButton(buttonContainerObj, "❌ CLOSE", () =>
        {
            ToggleDebugPanel();
        });
    }

    private void AddLabel(GameObject parent, string text)
    {
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(parent.transform, false);
        
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = text;
        labelText.fontSize = 16;
        labelText.color = Color.white;
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        LayoutElement layoutElement = labelObj.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 25;
    }

    private void AddSpacer(GameObject parent, float height)
    {
        GameObject spacerObj = new GameObject("Spacer");
        spacerObj.transform.SetParent(parent.transform, false);
        
        LayoutElement layoutElement = spacerObj.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = height;
    }

    private GameObject CreateInputField(GameObject parent, string placeholder, float width)
    {
        GameObject inputObj = new GameObject("InputField");
        inputObj.transform.SetParent(parent.transform, false);
        
        Image inputImage = inputObj.AddComponent<Image>();
        inputImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        inputField.textViewport = inputObj.GetComponent<RectTransform>();
        
        // 텍스트 자식 객체
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(inputObj.transform, false);
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = placeholder;
        textComponent.fontSize = 18;
        textComponent.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 0);
        textRect.offsetMax = new Vector2(-5, 0);
        
        inputField.textComponent = textComponent;
        inputField.text = "";
        
        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        LayoutElement layoutElement = inputObj.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = width;
        layoutElement.preferredHeight = 35;

        return inputObj;
    }

    private GameObject CreateButton(GameObject parent, string buttonText, Action onClick)
    {
        GameObject buttonObj = new GameObject("Button");
        buttonObj.transform.SetParent(parent.transform, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.5f, 0.8f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(() => onClick?.Invoke());
        
        // 색상 변화 효과
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.5f, 0.8f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.6f, 0.9f, 1f);
        colors.pressedColor = new Color(0.15f, 0.4f, 0.7f, 1f);
        button.colors = colors;
        
        // 텍스트
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = buttonText;
        textComponent.fontSize = 14;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 170;
        layoutElement.preferredHeight = 40;

        return buttonObj;
    }

    private void ToggleDebugPanel()
    {
        debugPanelActive = !debugPanelActive;
        debugCanvas.gameObject.SetActive(debugPanelActive);
        PlayerPrefs.SetInt(TOGGLE_DEBUG_PREF, debugPanelActive ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Debug panel {(debugPanelActive ? "opened" : "closed")}");
    }

    private void RefreshAllDisplays()
    {
        // UI 재생성하여 최신 데이터 표시
        Destroy(debugCanvas.gameObject);
        debugCanvas = null;
        CreateDebugUI();
    }
}
