using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GridManager gridManager;
    public BlockManager blockManager;
    public InputHandler inputHandler;
    public ScoreManager scoreManager;
    public SoundManager soundManager;
    public CameraManager cameraManager;
    public LeaderboardManager leaderboardManager;
    public UIManager uiManager;
    public static GameManager Instance;
    public ObjectPooler objectPooler;
    public int scoreLimit = 99999999;
    [NonSerialized] public bool isGameOver = false;
    public int reviveCount = 0;
    public int reviveLimit = 1;
    public int frameRate = 60;
    private float deltaTime; // FPS 계산을 위한 변수

    [Obsolete]
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 디바이스 주사율 확인
        float deviceRefreshRate;

        if (Screen.currentResolution.refreshRateRatio.denominator != 0)
        {
            deviceRefreshRate = (float)Screen.currentResolution.refreshRateRatio.numerator / Screen.currentResolution.refreshRateRatio.denominator;
        }
        else
        {
            deviceRefreshRate = Screen.currentResolution.refreshRate; // fallback to refreshRate property if ratio is invalid
        }
        Debug.Log("Device Refresh Rate: " + deviceRefreshRate);
        Debug.Log("Screen Resolution: " + Screen.currentResolution.width + "x" + Screen.currentResolution.height);
        Debug.Log("Refresh Rate Ratio: " + Screen.currentResolution.refreshRateRatio.numerator + "/" + 
              Screen.currentResolution.refreshRateRatio.denominator);
        
        // 주사율에 따라 타겟 프레임레이트 조정
        if (deviceRefreshRate > 0) 
        {
            frameRate = Mathf.RoundToInt(deviceRefreshRate);
            Debug.Log("Setting frame rate to device refresh rate: " + frameRate);
        }
        else 
        {
            Debug.Log("Using default frame rate: " + frameRate);
        }
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frameRate;

        // Make sure ObjectPooler is initialized early
        objectPooler = FindAnyObjectByType<ObjectPooler>();
        if (objectPooler == null)
        {
            Debug.LogError("ObjectPooler not found in the scene");
        }

        // Debug.unityLogger.logEnabled = false;
    }

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        // FPS 계산을 위해 deltaTime을 갱신합니다.
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    // void OnGUI()
    // {
    //     GUIStyle style = new GUIStyle();
    //     style.fontSize = 24;
    //     style.normal.textColor = Color.white;
    //     style.alignment = TextAnchor.UpperLeft;

    //     float msec = deltaTime * 1000.0f;
    //     float fps = 1.0f / deltaTime;
    //     string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

    //     // 화면 좌측 상단에 FPS 정보를 표시합니다.
    //     GUI.Label(new Rect(10, 300, 300, 40), text, style);
    // }

    private void InitializeGame()
    {
        gridManager.InitializeGrid();
        blockManager.InitializeGame();
        isGameOver = false;
    }

    public void RestartGame()
    {
        // Clear any game over blocks.
        gridManager.ClearGameOverBlocks();

        // Return generated blocks (if any) and remove them.
        if (blockManager.currentBlocks != null)
        {
            foreach (var block in blockManager.currentBlocks)
            {
                if (block != null)
                {
                    block.GetComponent<Block>().ReturnToPool();
                    Destroy(block);
                }
            }
        }

        // Reset grid: this returns all placed blocks to pool and resets grid.
        gridManager.ResetGrid();
        scoreManager.ResetScore();
        soundManager.ResetGame();
        uiManager.HideGameOverPanel();

        // Enable input.
        inputHandler.EnableDrag();

        // Restart game logic.
        InitializeGame();

        // Reset revive count.
        reviveCount = 0;
    }

    public void ReviveGame()
    {
        reviveCount++;
        isGameOver = false;
        // Clear game over block effects.
        gridManager.ClearGameOverBlocks();

        // Reactivate originally placed blocks so the board appears as it was.
        gridManager.ReactivateGridBlocks();
        
        // Remove the generated (preview) blocks and create 3 new blocks.
        blockManager.GenerateNewBlocks();
        uiManager.HideGameOverPanel();
        soundManager.PlayReviveSound();
        soundManager.PlayBGM();

        // Enable input.
        inputHandler.EnableDrag();
    }

    public bool CheckGameClear()
    {
        if (scoreManager.score == scoreLimit)
        {
            inputHandler.DisableDrag();

            isGameOver = true;

            StartCoroutine(GameClearCoroutine());

            return true;
        }
        return false;
    }

    // New method to start the game-over replacement coroutine.
    public bool CheckGameOver()
    {
        if (blockManager.remainBlockCount > 0)
        {
            for (int i = 0; i < blockManager.currentBlocks.Length; i++)
            {
                if (blockManager.currentBlocks[i] == null)
                    continue;

                for (int x = 0; x < gridManager.gridWidth; x++)
                {
                    for (int y = 0; y < gridManager.gridHeight; y++)
                    {
                        if (gridManager.CanPlaceBlock(x, y, blockManager.currentBlocks[i].GetComponent<Block>().shape))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        Debug.Log("Game Over!");

        // 1. 생성된 블록의 드래그를 비활성화 (InputHandler에 DisableDrag() 메서드를 구현하세요)
        inputHandler.DisableDrag();

        isGameOver = true;

        StartCoroutine(GameOverCoroutine());

        return true;
    }

    public IEnumerator GameOverCoroutine()
    {
        StartCoroutine(gridManager.ReplaceBlocksForGameOver());
        yield return StartCoroutine(soundManager.FadeOutBGMThenPlayGameOver());
        soundManager.PlayGameOverSound();

        bool tweenCompleted = false;
        uiManager.ShowGameOverPanel(false, () => tweenCompleted = true);
        yield return new WaitUntil(() => tweenCompleted);
        scoreManager.OnGameOver();
    }

    public IEnumerator GameClearCoroutine()
    {
        // StartCoroutine(gridManager.ReplaceBlocksForGameOver());
        yield return StartCoroutine(soundManager.FadeOutBGMThenPlayGameOver());
        soundManager.PlayGameClearSound();

        bool tweenCompleted = false;
        uiManager.ShowGameOverPanel(true, () => tweenCompleted = true);
        yield return new WaitUntil(() => tweenCompleted);
        scoreManager.OnGameOver();
    }

    public void QuitGame()
{
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
}
}