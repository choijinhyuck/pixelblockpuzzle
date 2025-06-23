using UnityEngine;
using DG.Tweening;

public class ClearLine : MonoBehaviour
{
    public float fadeDuration = 0.5f;      // Duration of the fade out tween
    public float fadeInDuration = 0.1f;      // Duration of the fade in tween

    void OnEnable()
    {
        // Get the SpriteRenderer component
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the sprite to be initially invisible
        Color color = sr.color;
        color.a = 0f;
        sr.color = color;

        // Create a DOTween sequence and link it to the GameObject to auto-kill if the GameObject is destroyed
        Sequence seq = DOTween.Sequence().SetLink(gameObject);

        // Fade in quickly then fade out
        seq.Append(sr.DOFade(1f, fadeInDuration));
        seq.Append(sr.DOFade(0f, fadeDuration));
        seq.OnComplete(() => {
            // Check if the GameObject still exists before attempting to destroy it
            if(gameObject != null)
                ObjectPooler.Instance.ReturnToPool(gameObject);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}