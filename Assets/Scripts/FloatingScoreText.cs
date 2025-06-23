using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class FloatingScoreText : MonoBehaviour
{
    public float moveUpDistance = 50f; // 캔버스 내 단위 (픽셀)
    public float duration = 1f;
    private TextMeshProUGUI scoreText;
    private RectTransform rectTransform;

    public void Awake()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }

    // worldPosition: 월드 스페이스 좌표
    // canvas: 오버레이 캔버스
    public void ShowScore(string score, Vector3 worldPosition, Canvas canvas)
    {
        scoreText.text = "+" + score;
        
        // 월드 좌표를 스크린 좌표로 변환한 후 캔버스 로컬 좌표로 변환
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, 
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main, out Vector2 canvasPos);
        rectTransform.anchoredPosition = canvasPos;
        
        // Reset alpha and scale
        scoreText.alpha = 1;
        rectTransform.localScale = Vector3.zero;
        
        // Create tween sequence:
        Sequence seq = DOTween.Sequence();
        
        // Pop effect: scale up then down
        seq.Append(rectTransform.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack));
        seq.Append(rectTransform.DOScale(1f, 0.1f).SetEase(Ease.InOutQuad));
        
        // Upward move (using anchoredPositionY) with slight shake and fade out concurrently
        seq.Append(
            DOTween.Sequence()
                .Append(rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + moveUpDistance, duration).SetEase(Ease.InSine))
                .Join(scoreText.DOFade(0, duration))
                .Join(rectTransform.DOShakeAnchorPos(duration, new Vector2(10f, 0), 10, 90, false))
        );
        
        // 작업 완료 후 오브젝트 풀로 반환
        seq.OnComplete(() => ObjectPooler.Instance.ReturnToPool(gameObject));
    }
}