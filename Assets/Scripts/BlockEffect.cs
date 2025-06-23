using UnityEngine;
using DG.Tweening;
using System;

public class BlockEffect : MonoBehaviour, IPoolable
{
    // Particle prefab to instantiate
    public GameObject particle;    // Number of particles to spawn
    [NonSerialized] public int particleCount = 10;
    // Speed/duration of the tween effect
    [NonSerialized] public float tweenDuration = 0.2f;
    // Distance for particle spread
    [NonSerialized] public float spreadDistance = 2.0f;

    private Animator animator;

    void OnEnable()
    {
        SyncAnimation();
    }

    // Optional: remove from Start if OnEnable covers your needs.
    void Start()
    {
        // SyncAnimation(); // Not needed if OnEnable is sufficient.
    }

    void SyncAnimation()
    {
        // Ensure the BlockManager has a singleton instance and a templateBlock with an Animator.
        Animator templateAnimator = GameManager.Instance.blockManager.templateBlock.GetComponent<Animator>();
        animator = GetComponent<Animator>();

        if(templateAnimator != null && animator != null)
        {
            // Get the current animator state info from the template block.
            AnimatorStateInfo templateStateInfo = templateAnimator.GetCurrentAnimatorStateInfo(0);

            // Sync the effect's animator state to the template block's state.
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(stateInfo.fullPathHash, 0, templateStateInfo.normalizedTime % 1);
        }
        else
        {
            Debug.LogWarning("Missing Animator component on template block or particle.");
        }
    }

    // Call this method to play the effect
    public void PlayEffect()
    {
        for (int i = 0; i < particleCount; i++)
        {
            // Instantiate particle at current position
            GameObject particleInstance = ObjectPooler.Instance.SpawnFromPool(particle, transform.position, Quaternion.identity);

            // Set a random scale between 0.5 and 1
            float randomScale = UnityEngine.Random.Range(0.3f, 0.6f);
            particleInstance.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

            // Calculate random direction
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;

            // Tween movement outward using an easing curve that starts slow then accelerates (Ease.InQuad)
            particleInstance.transform.DOMove(transform.position + (Vector3)(randomDir * spreadDistance), tweenDuration)
                                      .SetEase(DG.Tweening.Ease.Linear).onComplete = () => {ObjectPooler.Instance.ReturnToPool(particleInstance);};
        }
    }

    void Update()
    {
        
    }

    public void OnObjectSpawn()
    {
    }

    public void OnObjectReturn()
    {
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1.0f);
        transform.localScale = Vector3.one;
    }
}


