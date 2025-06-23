using UnityEngine;

public class Block : MonoBehaviour
{
    public BlockShape shape { get; private set; }
    public int blockOrder;
    public int blockColorIndex;
    private float blockScale = 0.55f;

    public void Initialize(BlockShape blockShape)
    {
        shape = blockShape;
        UpdateVisual();
    }

    public void ResetPositionAndScale()
    {
        transform.localScale = Vector3.one * blockScale;
        transform.position = new Vector3(0.5f + 3f * blockOrder, -2.5f, 0) - (Vector3)GetComponent<BoxCollider2D>().offset * blockScale;
        // transform.position = new Vector3(1 + 2.5f * blockOrder, -2.5f, 0) - (Vector3)GetComponent<BoxCollider2D>().offset * blockScale;
    }

    private void UpdateVisual()
    {
        // Create block units based on shape
        BlockManager blockManager = GameManager.Instance.blockManager;
        for (int x = 0; x < shape.width; x++)
        {
            for (int y = 0; y < shape.height; y++)
            {
                if (shape.shape[y, x] == 1)
                {
                    GameObject unit = ObjectPooler.Instance.SpawnFromPool(blockManager.blockUnitPrefabs[blockColorIndex], Vector3.zero, Quaternion.identity);
                    unit.transform.parent = transform;
                    unit.transform.localPosition = new Vector3(x, y, 0);
                }
            }
        }
    }

    public void ReplaceWithGameOverBlock()
    {
        GameObject gameoverBlockPrefab = GameManager.Instance.gridManager.gameOverBlockPrefab;
        if (gameoverBlockPrefab == null)
        {
            Debug.LogError("gameOverBlockPrefab not set in GameManager");
            return;
        }

        transform.localScale = Vector3.one;

        // Create block units based on shape
        for (int x = 0; x < shape.width; x++)
        {
            for (int y = 0; y < shape.height; y++)
            {
                if (shape.shape[y, x] == 1)
                {
                    GameObject unit = ObjectPooler.Instance.SpawnFromPool(gameoverBlockPrefab, Vector3.zero, Quaternion.identity);
                    unit.transform.parent = transform;
                    unit.transform.localPosition = new Vector3(x, y, 0);
                    unit.transform.localScale = Vector3.one;
                }
            }
        }

        ResetPositionAndScale();
    }

    public void ReturnToPool()
    {
        while(transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            child.SetParent(null);
            ObjectPooler.Instance.ReturnToPool(child.gameObject);
        }
    }
}