using TMPro;
using UnityEngine;
using DG.Tweening;  // DOTween 네임스페이스
using System;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    // Add a new field for Best Score Text UI element
    public TextMeshProUGUI bestScoreText;  

    public int score = 0;
    [NonSerialized] public int comboCount = 0;         // 현재 콤보 수
    [NonSerialized] public int lastClearScore;
    private int blocksSinceLastClear = 0;  // 마지막 라인 클리어 이후 놓은 블록 수
    private int bestScore = 0;  // 저장된 최고 점수

    // tween 제어용 변수
    private Tween comboTween;
    private Tween scoreTween;  // score용 tween 변수
    private Tween scoreColorTween;  // 점수 텍스트의 색상 변경 tween

    // 이전 콤보 값을 저장 (초기값은 -1로 하여 최초 업데이트가 무조건 수행되게 함)
    private int lastComboCount = -1;
    private Color defaultScoreColor;
    
    void Start()
    {
        // 기본 색상 저장 (에디터에서 설정된 값)
        defaultScoreColor = scoreText.color;
        LoadBestScore();
        ResetScore();
    }

    // Load best score from PlayerPrefs.
    // 만약 저장된 날짜가 오늘과 다르다면 best score를 초기화합니다.
    private void LoadBestScore()
    {
        // 날짜에 관계없이 저장된 BestScore를 불러옵니다.
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateBestScoreText();
    }
    
    // Update the bestScoreText UI.
    private void UpdateBestScoreText()
    {
        bestScoreText.text = "<color=yellow>Best</color> " + bestScore.ToString();
        
        // Ensure the scale is reset.
        bestScoreText.transform.localScale = Vector3.one;
        
        // Tween effect: scale up then back to normal (Yoyo loop).
        bestScoreText.transform.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);
    }

    // 블록을 놓았을 때 호출 (예: Block 배치 후)
    // blockUnitCount는 해당 블록의 유닛 개수 등 배치 점수를 계산할 때 사용됩니다.
    public void AddBlockScore(int blockUnitCount)
    {
        score += blockUnitCount;
        score = Mathf.Min(score, GameManager.Instance.scoreLimit);
        if (blocksSinceLastClear != -1)
        {
            blocksSinceLastClear++;
        }

        // Update best score if needed
        if (score > bestScore)
        {
            bestScore = score;
            UpdateBestScoreText();
        }

        Debug.Log("Block placed. Score increased by " + blockUnitCount + ". Total score: " + score);
        UpdateScoreText();
        UpdateComboText();
    }

    // 라인 클리어 시 호출 (예: GridManager.CheckAndClearFullLines에서)
    public void AddLineClearScore(int linesCleared)
    {
        score += linesCleared * 10;
        score = Mathf.Min(score, GameManager.Instance.scoreLimit);
        lastClearScore = linesCleared * 10;

        // 마지막 라인 클리어 이후 4블록 안에 성공한 클리어면 콤보 증가
        if (blocksSinceLastClear >= 0 && blocksSinceLastClear <= 4)
        {
            comboCount++;  // 콤보 증가
        }

        score += comboCount * 10 * linesCleared;
        score = Mathf.Min(score, GameManager.Instance.scoreLimit);
        lastClearScore += comboCount * 10 * linesCleared;

        // Update best score if needed
        if (score > bestScore)
        {
            bestScore = score;
            UpdateBestScoreText();
        }

        // 라인 클리어가 발생했으므로 블록 배치 횟수 초기화
        blocksSinceLastClear = 0;
        UpdateScoreText();
        UpdateComboText();
    }

    public void ResetCombo()
    {
        if (blocksSinceLastClear == 4)
        {
            comboCount = 0;
            blocksSinceLastClear = -1;
            GameManager.Instance.soundManager.ResetClearClipIndex();
        }

        UpdateScoreText();
        UpdateComboText();
    }

    public void ResetScore()
    {
        score = 0;
        comboCount = 0;
        blocksSinceLastClear = -1;
        lastComboCount = -1;
        
        LoadBestScore();

        // comboText.gameObject.SetActive(false);
        UpdateScoreText();
        UpdateComboText();
    }

    // Call this method on game over to save the best score in PlayerPrefs.
    public void OnGameOver()
    {
        PlayerPrefs.SetInt("BestScore", bestScore);
        PlayerPrefs.Save();

        GameManager.Instance.leaderboardManager.SubmitScore(score);
    }

    private void UpdateScoreText()
    {
        scoreText.text = score.ToString();
        
        // score 변경 시 스케일 tween 효과 적용 (기존 tween 종료 후 재시작)
        if (scoreTween != null)
            scoreTween.Kill();
        scoreText.transform.localScale = Vector3.one * 0.8f;
        float targetScale = (comboCount > 0) ? 1.3f : 1f;
        scoreTween = scoreText.transform.DOScale(targetScale, 0.3f).SetEase(Ease.OutBack);

        // comboCount가 0 초과면 빨간색 계열의 불타는 효과(tween으로 색상 반복 변경) 적용, 아니면 원복
        if (comboCount > 0)
        {
            if (scoreColorTween == null)
            {
                // 불타는 효과: 기본 색상에서 빨간색 계열로 반복 애니메이션 (Yoyo loop)
                scoreColorTween = DOTween.To(() => scoreText.color, x => scoreText.color = x, new Color(1f, 0f, 0f), 0.5f)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }
        else
        {
            if (scoreColorTween != null)
            {
                scoreColorTween.Kill();
                scoreColorTween = null;
                scoreText.color = defaultScoreColor;
            }
        }
    }

    private void UpdateComboText()
    {
        // 콤보 값이 이전과 동일하면 아무것도 하지 않음
        if (comboCount == lastComboCount)
            return;
        lastComboCount = comboCount;

        // 콤보 수가 0이면 tween 취소 후 텍스트 비활성화
        if (comboCount == 0)
        {
            if (comboTween != null) comboTween.Kill();
            comboText.gameObject.SetActive(false);
            return;
        }

        // 콤보 값 업데이트 및 텍스트 활성화
        comboText.text = "<color=purple><size=125>" 
            + comboCount.ToString()
            + "</size></color><br>COMBO";
        comboText.gameObject.SetActive(true);

        // 이전 tween이 있다면 취소
        if (comboTween != null) comboTween.Kill();
        
        // tween 효과: 텍스트의 크기를 작게 설정 후 팝업 애니메이션,
        // 0.5초 애니메이션 후 2초 후 비활성화
        comboText.transform.localScale = Vector3.zero;
        comboTween = comboText.transform.DOScale(1f, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // tween 중 새로운 콤보 업데이트가 있을 경우를 대비해 null 체크
                comboTween = DOVirtual.DelayedCall(1f, () => 
                {
                    comboText.gameObject.SetActive(false);
                });
            });
    }
}