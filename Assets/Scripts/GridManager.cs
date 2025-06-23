using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int gridWidth = 8;
    public int gridHeight = 8;
    public GameObject clearLineRowPrefab;     // Prefab for row clear effect
    public GameObject clearLineColumnPrefab;  // Prefab for column clear effect
    public GameObject gameOverBlockPrefab;    // Prefab to show on game over replacement
    public GameObject[,] grid { get; private set; }

    // New list to hold spawned GameOverBlock objects.
    private List<GameObject> gameOverBlocks = new List<GameObject>();

    public void InitializeGrid()
    {
        grid = new GameObject[gridWidth, gridHeight];
    }

    public bool IsIndexValid(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    public void SetBlock(int x, int y, GameObject block)
    {
        if (!IsIndexValid(x, y)) return;
        grid[x, y] = block;
    }

    public bool CheckAndClearFullLines()
    {
        bool cleared = false;
        int totalRowsCleared = 0;
        int totalColumnsCleared = 0;
        HashSet<Vector2Int> positionsToClear = new HashSet<Vector2Int>();
        List<int> clearedRows = new List<int>();      // store cleared rows
        List<int> clearedColumns = new List<int>();   // store cleared columns

        // Check rows and mark positions
        for (int y = 0; y < gridHeight; y++)
        {
            if (IsRowFull(y))
            {
                totalRowsCleared++;
                clearedRows.Add(y);
                for (int x = 0; x < gridWidth; x++)
                {
                    positionsToClear.Add(new Vector2Int(x, y));
                }
            }
        }
        // Check columns and mark positions
        for (int x = 0; x < gridWidth; x++)
        {
            if (IsColumnFull(x))
            {
                totalColumnsCleared++;
                clearedColumns.Add(x);
                for (int y = 0; y < gridHeight; y++)
                {
                    positionsToClear.Add(new Vector2Int(x, y));
                }
            }
        }

        foreach (var pos in positionsToClear)
        {
            if (grid[pos.x, pos.y] != null)
            {
                var effect = grid[pos.x, pos.y].GetComponent<BlockEffect>();
                if (effect != null)
                {
                    // Return to pool instead of destroying
                    effect.PlayEffect();
                    ObjectPooler.Instance.ReturnToPool(grid[pos.x, pos.y]);
                }
                else
                {
                    ObjectPooler.Instance.ReturnToPool(grid[pos.x, pos.y]);
                }
                grid[pos.x, pos.y] = null;
                cleared = true;
            }
        }

        if (cleared)
        {
            int totalLinesCleared = totalRowsCleared + totalColumnsCleared;
            GameManager.Instance.scoreManager.AddLineClearScore(totalLinesCleared);
            GameManager.Instance.soundManager.PlayLineClearSound();

            // 카메라 흔들림 효과 호출 추가
            GameManager.Instance.cameraManager.ShakeCamera();

            // Replace Instantiate with object pooling for each cleared row
            foreach (int row in clearedRows)
            {
                Vector3 spawnPosition = new Vector3(-0.5f, row - 0.5f, 0);
                ObjectPooler.Instance.SpawnFromPool(clearLineRowPrefab, spawnPosition, Quaternion.identity);
            }

            // Replace Instantiate with object pooling for each cleared column
            foreach (int column in clearedColumns)
            {
                Vector3 spawnPosition = new Vector3(column - 0.5f, -0.5f, 0);
                ObjectPooler.Instance.SpawnFromPool(clearLineColumnPrefab, spawnPosition, Quaternion.identity);
            }
        }

        return cleared;
    }

    private bool IsRowFull(int row)
    {
        if (row < 0 || row >= gridHeight) return false;
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, row] == null) return false;
        }
        return true;
    }

    private bool IsColumnFull(int column)
    {
        if (column < 0 || column >= gridWidth) return false;
        for (int y = 0; y < gridHeight; y++)
        {
            if (grid[column, y] == null) return false;
        }
        return true;
    }

    public bool CanPlaceBlock(int x, int y, BlockShape shape)
    {
        if (shape == null) return false;
        if (x < 0 || y < 0 || x + shape.width > gridWidth || y + shape.height > gridHeight)
            return false;

        for (int i = 0; i < shape.width; i++)
        {
            for (int j = 0; j < shape.height; j++)
            {
                if (shape.shape[j, i] == 1)
                {
                    if (!IsIndexValid(x + i, y + j) || grid[x + i, y + j] != null)
                        return false;
                }
            }
        }
        return true;
    }

    public void ResetGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    // Return to pool instead of destroying
                    ObjectPooler.Instance.ReturnToPool(grid[x, y]);
                    grid[x, y] = null;
                }
            }
        }
    }

    public (List<int> clearedRows, List<int> clearedColumns) GetPotentialClearedLines(int startX, int startY, BlockShape shape)
    {
        bool[,] simulated = new bool[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                simulated[x, y] = grid[x, y] != null;
            }
        }
        for (int i = 0; i < shape.width; i++)
        {
            for (int j = 0; j < shape.height; j++)
            {
                if (shape.shape[j, i] == 1)
                {
                    int gx = startX + i;
                    int gy = startY + j;
                    if (gx >= 0 && gy >= 0 && gx < gridWidth && gy < gridHeight)
                    {
                        simulated[gx, gy] = true;
                    }
                }
            }
        }
        List<int> clearedRows = new List<int>();
        for (int y = 0; y < gridHeight; y++)
        {
            bool full = true;
            for (int x = 0; x < gridWidth; x++)
            {
                if (!simulated[x, y])
                {
                    full = false;
                    break;
                }
            }
            if (full) clearedRows.Add(y);
        }
        List<int> clearedColumns = new List<int>();
        for (int x = 0; x < gridWidth; x++)
        {
            bool full = true;
            for (int y = 0; y < gridHeight; y++)
            {
                if (!simulated[x, y])
                {
                    full = false;
                    break;
                }
            }
            if (full) clearedColumns.Add(x);
        }
        return (clearedRows, clearedColumns);
    }

    // New method: disable all placed blocks instead of returning them to pool immediately.
    public void DeactivateGridBlocks()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y].SetActive(false);
                }
            }
        }
    }

    // New method: reactivate all placed blocks during a revive.
    public void ReactivateGridBlocks()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y].SetActive(true);
                }
            }
        }
    }

    // Updated coroutine for game over effects.
    public IEnumerator ReplaceBlocksForGameOver()
    {
        // Instead of returning blocks immediately, just disable them.
        DeactivateGridBlocks();

        // Spawn game over block effects at each grid position.
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    // Do not return the block – keep it disabled for revival.
                    Vector3 spawnPosition = new Vector3(x, y - 0.5f, 0);
                    GameObject goBlock = ObjectPooler.Instance.SpawnFromPool(gameOverBlockPrefab, spawnPosition, Quaternion.identity);
                    goBlock.transform.localScale = Vector3.one;
                    // Add to gameOverBlocks for later clearing.
                    gameOverBlocks.Add(goBlock);
                }
            }
            // Wait one frame (or a short time) after processing each row.
            yield return new WaitForSecondsRealtime(0.05f);
        }

        var currentBlocks = GameManager.Instance.blockManager.currentBlocks;

        for (int i = 0; i < currentBlocks.Length; i++)
        {
            if (currentBlocks[i] != null)
            {
                currentBlocks[i].GetComponent<Block>().ReturnToPool();
                currentBlocks[i].GetComponent<Block>().ReplaceWithGameOverBlock();
            }
        }
    }

    // New method to clear all GameOverBlock objects.
    public void ClearGameOverBlocks()
    {
        foreach (GameObject goBlock in gameOverBlocks)
        {
            if (goBlock != null)
            {
                ObjectPooler.Instance.ReturnToPool(goBlock);
            }
        }
        gameOverBlocks.Clear();
    }
}
