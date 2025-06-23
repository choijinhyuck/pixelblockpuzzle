using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using System.Collections; // Added for tween effects
using System;

public class UIManager : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject safeareaPanel;
    public GameObject gameOverPanel;
    public GameObject leaderboardPanel;
    public GameObject restartButton;
    public GameObject reviveButton;
    public GameObject rankButton;
    public Canvas canvas;
    public TextMeshProUGUI gameoverTMP;
    public TextMeshProUGUI gameclearTMP;

    // New fields for vibration UI
    public Button vibrationToggleButton;
    public TextMeshProUGUI vibrationToggleButtonText;
    
    // New field for Exit Popup
    public GameObject exitPopup;

    private Vector3 restartButtonOriginalScale;
    private Vector3 rankButtonOriginalScale;
    private Vector3 reviveButtonOriginalScale;

    void Start()
    {
        if (menuPanel.activeSelf)
        {
            menuPanel.SetActive(false);
        }

        if (gameOverPanel.activeSelf)
        {
            gameOverPanel.SetActive(false);
        }

        if (exitPopup.activeSelf)
        {
            exitPopup.SetActive(false);
        }

        if (leaderboardPanel.activeSelf)
        {
            exitPopup.SetActive(false);
        }

        // Store the original scales for later use
        restartButtonOriginalScale = restartButton.transform.localScale;
        rankButtonOriginalScale = rankButton.transform.localScale;
        reviveButtonOriginalScale = reviveButton.transform.localScale;
        reviveButton.SetActive(false); // ensure it's disabled by default

        ApplySafeArea(safeareaPanel);

        // Set up the vibration toggle button listener and text
        if (vibrationToggleButton != null)
        {
            UpdateVibrationToggleButtonText();
        }
    }

    void Update()
    {
        // Handle mobile's back button (escape key) press.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var activePanel = IsAnyPanelActive();
            if (activePanel != null)
            {
                OnClickPanelCloseButton(activePanel);
            }
            else
            {
                // No active panels -> show exit popup
                if (exitPopup != null)
                {
                    OnClickPanelToggleButton(exitPopup);
                }
            }
        }
    }

    private GameObject IsAnyPanelActive()
    {
        if (leaderboardPanel.activeSelf) return leaderboardPanel;
        if (menuPanel.activeSelf) return menuPanel;
        if (exitPopup.activeSelf) return exitPopup;

        return null;
    }

    public void OnClickPanelToggleButton(GameObject panel)
    {
        if (!panel.activeSelf)
        {
            panel.SetActive(true);
            panel.transform.localScale = Vector3.zero;
            panel.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
            GameManager.Instance.inputHandler.DisableDrag();
        }
    }

    public void OnClickPanelCloseButton(GameObject panel)
    {
        if (panel.activeSelf)
        {
            if (DOTween.IsTweening(panel.transform))
            {
                return;
            }
            panel.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack).OnComplete(() =>
            {
                panel.SetActive(false);
                if (!GameManager.Instance.isGameOver && menuPanel.activeSelf == false)
                {
                    GameManager.Instance.inputHandler.EnableDrag();
                }
            });
        }
    }

    public void OnClickBGMButton()
    {
        GameManager.Instance.soundManager.ToggleBGM();
    }

    public void OnClickFXButton()
    {
        GameManager.Instance.soundManager.ToggleFX();
    }

    public void ApplySafeArea(GameObject panel)
    {
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }

    public void ShowGameOverPanel(bool isGameClear, Action onTweenComplete = null)
    {
        restartButton.SetActive(false);
        rankButton.SetActive(false);
        reviveButton.SetActive(false);
        
        if (!isGameClear)
        {
            gameoverTMP.gameObject.SetActive(true);
            gameclearTMP.gameObject.SetActive(false);
        }
        else
        {
            gameoverTMP.gameObject.SetActive(false);
            gameclearTMP.gameObject.SetActive(true);
        }

        gameOverPanel.SetActive(true);
        gameOverPanel.transform.localScale = Vector3.zero;

        gameOverPanel.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            restartButton.SetActive(true);
            rankButton.SetActive(true);
            restartButton.GetComponent<Button>().interactable = true;
            rankButton.GetComponent<Button>().interactable = true;
            restartButton.transform.localScale = Vector3.zero;
            rankButton.transform.localScale = Vector3.zero;
            restartButton.transform.DOScale(restartButtonOriginalScale, 0.3f).SetEase(Ease.OutBack);
            rankButton.transform.DOScale(rankButtonOriginalScale, 0.3f).SetEase(Ease.OutBack);

            if (!isGameClear && GameManager.Instance.reviveCount < GameManager.Instance.reviveLimit)
            {
                reviveButton.SetActive(true);
                reviveButton.GetComponent<Button>().interactable = false;
                reviveButton.GetComponent<RewardedAdsButton>().LoadAd();
                reviveButton.transform.localScale = Vector3.zero;
                reviveButton.transform.DOScale(reviveButtonOriginalScale, 0.3f).SetEase(Ease.OutBack);
            }

            onTweenComplete.Invoke();
        });
    }

    public void HideGameOverPanel()
    {
        if (gameOverPanel.activeSelf)
        {
            restartButton.GetComponent<Button>().interactable = false;
            rankButton.GetComponent<Button>().interactable = false;
            reviveButton.GetComponent<Button>().interactable = false;
            restartButton.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.OutBack);
            rankButton.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.OutBack);
            gameOverPanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                gameOverPanel.SetActive(false);
            });
        }
    }
    
    // New method for toggling vibration via the UI button.
    public void OnClickVibrationToggleButton()
    {
        VibrateManager.Instance.ToggleVibration();
        UpdateVibrationToggleButtonText();
    }

    private void UpdateVibrationToggleButtonText()
    {
        if(vibrationToggleButtonText != null)
        {
            if (VibrateManager.Instance.isVibrationEnabled)
                vibrationToggleButtonText.text = "on";
            else
                vibrationToggleButtonText.text = "off";
        }
    }
}
