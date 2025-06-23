using UnityEngine;

public class BlockUnit : MonoBehaviour, IPoolable
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnObjectSpawn()
    {
        // Reset any state when spawned from pool
        if (spriteRenderer != null)
        {
            // Reset any sprite renderer properties if needed
        }
    }

    public void OnObjectReturn()
    {
        // Reset any state when returned to pool
    }
}
