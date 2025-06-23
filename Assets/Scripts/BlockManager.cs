using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class BlockManager : MonoBehaviour
{
    [SerializeField] private float random_weight = 2f; // 가중치 기본값 2로 설정
    public GameObject[] blockUnitPrefabs;
    public GameObject templateBlock;
    public GameObject[] currentBlocks { get; private set; }
    public BlockShape[] currentBlockShapes { get; private set; }
    [NonSerialized] public int remainBlockCount = 3;
    private GridManager gridManager;
    private List<BlockShape> possibleBlockShapes = new List<BlockShape>();

    // 캐시: 각 BlockShape에 대해 빈 그리드 배치 후보 좌표들
    private Dictionary<BlockShape, List<Vector2Int>> baseValidPlacementsCache = new Dictionary<BlockShape, List<Vector2Int>>();

    void Start()
    {
        gridManager = GameManager.Instance.gridManager;
    }

    public void InitializeGame()
    {
        InitializeBlockShapes();
        GenerateNewBlocks();
    }

    private void InitializeBlockShapes()
    {
        // Define block shapes in a more structured way
        int[][,] blockDefinitions = new int[][,]
        {
            // 1 block
            new int[,] {
                {1}
                },
            
            // 2 blocks
            new int[,] {
                {1,1}
                },
            new int[,] {
                {1},
                {1}
                },
            new int[,] {
                {1,0},
                {0,1}
                },
            new int[,] {
                {0,1},
                {1,0}
            },
            
            // 3 blocks
            new int[,] {
                {1,1,1}
                },
            new int[,] {
                {1},
                {1},
                {1}
                },
            new int[,] {
                {1,1},
                {1,0}
                },
            new int[,] {
                {1,1},
                {0,1}
                },
            new int[,] {
                {0,1},
                {1,1}
                },
            new int[,] {
                {1,0},
                {1,1}
                },
            new int[,] {
                {1,0,0},
                {0,1,0},
                {0,0,1}
                },
            new int[,] {
                {0,0,1},
                {0,1,0},
                {1,0,0}
                },
            
            // 4 blocks
            new int[,] {
                {1,1},
                {1,1}
                },
            new int[,] {
                {1,1,1,1}
                },
            new int[,] {
                {1},
                {1},
                {1},
                {1}
                },
            new int[,] {
                {1,0},
                {1,0},
                {1,1}
                },
            new int[,] {
                {1,1},
                {1,0},
                {1,0}
                },
            new int[,] {
                {0,1},
                {0,1},
                {1,1}
                },
            new int[,] {
                {1,1},
                {0,1},
                {0,1}
                },
            new int[,] {
                {1,1,1},
                {1,0,0}
                },
            new int[,] {
                {1,1,1},
                {0,0,1}
                },
            new int[,] {
                {1,0,0},
                {1,1,1}
                },
            new int[,] {
                {0,0,1},
                {1,1,1}
                },
            new int[,] {
                {1,1,1},
                {0,1,0}
                },
            new int[,] {
                {0,1,0},
                {1,1,1}
                },
            new int[,] {
                {1,0},
                {1,1},
                {1,0}
                },
            new int[,] {
                {0,1},
                {1,1},
                {0,1}
                },
            new int[,] {
                {1,1,0},
                {0,1,1}
                },
            new int[,] {
                {0,1,1},
                {1,1,0}
                },
            new int[,] {
                {0,1},
                {1,1},
                {1,0}
                },
            new int[,] {
                {1,0},
                {1,1},
                {0,1}
                },

            
            // 5 blocks
            new int[,] {
                {1,1,1,1,1}
                },
            new int[,] {
                {1},
                {1},
                {1},
                {1},
                {1}
                },
            new int[,] {
                {1,1,1,1},
                {1,0,0,0}
                },
            new int[,] {
                {1,1,1,1},
                {0,0,0,1}
                },
            new int[,] {
                {1,0,0,0},
                {1,1,1,1}
                },
            new int[,] {
                {0,0,0,1},
                {1,1,1,1}
                },
            new int[,] {
                {1,1,1},
                {1,0,0},
                {1,0,0}
                },
            new int[,] {
                {1,1,1},
                {0,0,1},
                {0,0,1}
                },
            new int[,] {
                {1,0,0},
                {1,0,0},
                {1,1,1}
                },
            new int[,] {
                {0,0,1},
                {0,0,1},
                {1,1,1}
                },
            new int[,] {
                {1,1,1},
                {0,1,0},
                {0,1,0}
                },
            new int[,] {
                {0,1,0},
                {0,1,0},
                {1,1,1}
                },
            new int[,] {
                {1,0,0},
                {1,1,1},
                {1,0,0}
                },
            new int[,] {
                {0,0,1},
                {1,1,1},
                {0,0,1}
                },
            new int[,] {
                {0,1,0},
                {1,1,1},
                {0,1,0}
                },
            new int[,] {
                {1,0,1},
                {1,1,1}
                },
            new int[,] {
                {1,1,1},
                {1,0,1}
                },
            new int[,] {
                {1,1},
                {1,0},
                {1,1}
                },
            new int[,] {
                {1,1},
                {0,1},
                {1,1}
                },
            
            // 6 blocks
            new int[,] {
                {1,1,1},
                {1,1,1}
                },
            new int[,] {
                {1,1},
                {1,1},
                {1,1}
                },
            // new int[,] {
            //     {1,1,1,1},
            //     {1,0,0,0},
            //     {1,0,0,0}
            //     },
            // new int[,] {
            //     {1,1,1,1},
            //     {0,0,0,1},
            //     {0,0,0,1}
            //     },
            // new int[,] {
            //     {1,0,0,0},
            //     {1,0,0,0},
            //     {1,1,1,1}
            //     },
            // new int[,] {
            //     {0,0,0,1},
            //     {0,0,0,1},
            //     {1,1,1,1}
            //     },
            // new int[,] {
            //     {1,1,1},
            //     {1,0,0},
            //     {1,0,0},
            //     {1,0,0}
            //     },
            // new int[,] {
            //     {1,1,1},
            //     {0,0,1},
            //     {0,0,1},
            //     {0,0,1}
            //     },
            // new int[,] {
            //     {1,0,0},
            //     {1,0,0},
            //     {1,0,0},
            //     {1,1,1}
            //     },
            // new int[,] {
            //     {0,0,1},
            //     {0,0,1},
            //     {0,0,1},
            //     {1,1,1}
            //     },
            // new int[,] {
            //     {1,1,1,1,1},
            //     {1,0,0,0,0}
            //     },
            // new int[,] {
            //     {1,1,1,1,1},
            //     {0,0,0,0,1}
            //     },
            // new int[,] {
            //     {1,0,0,0,0},
            //     {1,1,1,1,1}
            //     },
            // new int[,] {
            //     {0,0,0,0,1},
            //     {1,1,1,1,1}
            //     },
            // new int[,] {
            //     {1,1},
            //     {1,0},
            //     {1,0},
            //     {1,0},
            //     {1,0}
            //     },
            // new int[,] {
            //     {1,1},
            //     {0,1},
            //     {0,1},
            //     {0,1},
            //     {0,1}
            //     },
            // new int[,] {
            //     {1,0},
            //     {1,0},
            //     {1,0},
            //     {1,0},
            //     {1,1}
            //     },
            // new int[,] {
            //     {0,1},
            //     {0,1},
            //     {0,1},
            //     {0,1},
            //     {1,1}
            //     },


            // 7 blocks
            // new int[,] {
            //     {1,1,1,1},
            //     {1,0,0,0},
            //     {1,0,0,0},
            //     {1,0,0,0}
            //     },
            // new int[,] {
            //     {1,1,1,1},
            //     {0,0,0,1},
            //     {0,0,0,1},
            //     {0,0,0,1}
            //     },
            // new int[,] {
            //     {1,0,0,0},
            //     {1,0,0,0},
            //     {1,0,0,0},
            //     {1,1,1,1}
            //     },
            // new int[,] {
            //     {0,0,0,1},
            //     {0,0,0,1},
            //     {0,0,0,1},
            //     {1,1,1,1}
            //     },
            // new int [,] {
            //     {1,0,0},
            //     {1,0,0},
            //     {1,1,1},
            //     {1,0,0},
            //     {1,0,0}
            //     },
            // new int [,] {
            //     {0,0,1},
            //     {0,0,1},
            //     {1,1,1},
            //     {0,0,1},
            //     {0,0,1}
            //     },
            // new int [,] {
            //     {1,1,1,1,1},
            //     {0,0,1,0,0},
            //     {0,0,1,0,0}
            //     },
            // new int [,] {
            //     {0,0,1,0,0},
            //     {0,0,1,0,0},
            //     {1,1,1,1,1}
            //     },
            // new int [,] {
            //     {0,0,1,1,1},
            //     {0,0,1,0,0},
            //     {1,1,1,0,0}
            //     },
            // new int [,] {
            //     {1,1,1,0,0},
            //     {0,0,1,0,0},
            //     {0,0,1,1,1}
            //     },
            // new int [,] {
            //     {1,0,0},
            //     {1,0,0},
            //     {1,1,1},
            //     {0,0,1},
            //     {0,0,1}
            //     },
            // new int [,] {
            //     {0,0,1},
            //     {0,0,1},
            //     {1,1,1},
            //     {1,0,0},
            //     {1,0,0}
            //     },
            // new int [,] {
            //     {1,1,1},
            //     {0,0,1},
            //     {1,1,1}
            //     },
            // new int [,] {
            //     {1,1,1},
            //     {1,0,0},
            //     {1,1,1}
            //     },
            // new int [,] {
            //     {1,0,1},
            //     {1,0,1},
            //     {1,1,1}
            //     },
            // new int [,] {
            //     {1,1,1},
            //     {1,0,1},
            //     {1,0,1}
            //     },

            // 8 blocks
            // new int [,] {
            //     {1,1,1},
            //     {1,0,1},
            //     {1,1,1}
            //     },
            // new int [,] {
            //     {1,1,1,1},
            //     {1,1,1,1}
            //     },
            // new int [,] {
            //     {1,1},
            //     {1,1},
            //     {1,1},
            //     {1,1}
            //     },

            // 9 blocks
            new int[,] {
                {1,1,1},
                {1,1,1},
                {1,1,1}
                }
        };

        // Add all defined shapes to possibleBlockShapes
        foreach (int[,] shape in blockDefinitions)
        {
            possibleBlockShapes.Add(new BlockShape { shape = shape });
        }

        foreach (var block in possibleBlockShapes)
        {
            block.CalculateCount();
        }
    }

    public void GenerateNewBlocks()
    {
        const int MAX_ATTEMPTS = 10;
        int attempts = 0;

        while (attempts < MAX_ATTEMPTS)
        {
            attempts++;
            if (TryGenerateBlocks())
                return;
        }

        // 최대 시도 횟수를 초과하면 가장 작은 블록들로 시도
        FallbackToSimpleBlocks();
    }

    private bool TryGenerateBlocks()
    {
        if (currentBlocks != null)
        {
            foreach (var currentBlock in currentBlocks)
            {
                if (currentBlock != null)
                {
                    currentBlock.GetComponent<Block>().ReturnToPool();
                    Destroy(currentBlock);
                }
            }
        }

        currentBlocks = new GameObject[3];
        currentBlockShapes = new BlockShape[3];
        if (gridManager == null)
        {
            gridManager = GameManager.Instance.gridManager;
        }
        
        // 현재 그리드를 한 번만 복제하여 사용
        bool[,] baseGrid = new bool[gridManager.gridWidth, gridManager.gridHeight];
        for (int x = 0; x < gridManager.gridWidth; x++)
            for (int y = 0; y < gridManager.gridHeight; y++)
                baseGrid[x, y] = gridManager.grid[x, y] != null;
        
        List<BlockShape> selectedBlocks = new List<BlockShape>();
        
        if (SelectBlocksRecursive(baseGrid, 0, selectedBlocks))
        {
            currentBlockShapes = selectedBlocks.ToArray();
            CreateBlockObjects();
            return true;
        }
        
        //Debug.Log("Failed to find valid block combination");
        return false;
    }

    private bool SelectBlocksRecursive(bool[,] grid, int blockIndex, List<BlockShape> selected)
    {
        if (blockIndex >= 3)
            return true;
        
        // 현재 그리드 상태에서 배치 가능한 블록들을 가져옵니다.
        var validBlocks = possibleBlockShapes.Where(block => HasValidPlacementInTest(grid, block)).ToList();
        // weighted 선택 시 중복 시도를 피하기 위해 복사본을 사용합니다.
        List<BlockShape> remainingBlocks = new List<BlockShape>(validBlocks);
        
        while (remainingBlocks.Count > 0)
        {
            // 높은 count의 blockshape이 우선 선택되도록 가중치 로직을 이용합니다.
            BlockShape selectedBlock = SelectWeightedRandomBlock(remainingBlocks);
            remainingBlocks.Remove(selectedBlock);
            
            bool[,] gridCopy = CloneGrid(grid);
            if (TryPlaceBlockAnywhere(gridCopy, selectedBlock))
            {
                CheckAndClearLines(gridCopy);
                selected.Add(selectedBlock);
                if (SelectBlocksRecursive(gridCopy, blockIndex + 1, selected))
                    return true;
                selected.RemoveAt(selected.Count - 1);
            }
        }
        
        return false;
    }

    private void FallbackToSimpleBlocks()
    {
        currentBlocks = new GameObject[3];
        currentBlockShapes = new BlockShape[3];

        // 가장 작은 블록(1x1)을 선택
        var simpleBlock = possibleBlockShapes.OrderBy(b => b.count).First();

        for (int i = 0; i < 3; i++)
        {
            currentBlockShapes[i] = simpleBlock;
        }

        CreateBlockObjects();
    }

    private void CreateBlockObjects()
    {
        // Randomly shuffle the currentBlockShapes array
        for (int i = currentBlockShapes.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            BlockShape temp = currentBlockShapes[i];
            currentBlockShapes[i] = currentBlockShapes[j];
            currentBlockShapes[j] = temp;
        }

        // Create a list of available unique color indices
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < blockUnitPrefabs.Length; i++)
        {
            availableIndices.Add(i);
        }
        
        for (int i = 0; i < 3; i++)
        {
            GameObject newBlock = new GameObject($"Block_{i}");
            newBlock.tag = "Block";

            Block blockComponent = newBlock.AddComponent<Block>();

            // Randomly select a unique color index and remove it to avoid duplicates
            int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
            blockComponent.blockColorIndex = availableIndices[randomIndex];
            availableIndices.RemoveAt(randomIndex);

            blockComponent.Initialize(currentBlockShapes[i]);

            BoxCollider2D collider = newBlock.AddComponent<BoxCollider2D>();

            collider.size = new Vector2(5, 5);
            collider.offset = new Vector2(
                (currentBlockShapes[i].width - 1) * 0.5f,
                (currentBlockShapes[i].height) * 0.5f
            );

            // collider.size = new Vector2(currentBlockShapes[i].width, currentBlockShapes[i].height);
            // collider.offset = new Vector2(
            //     (currentBlockShapes[i].width - 1) * 0.5f,
            //     (currentBlockShapes[i].height) * 0.5f
            // );

            blockComponent.blockOrder = i;

            blockComponent.ResetPositionAndScale();

            currentBlocks[i] = newBlock;
        }
        remainBlockCount = 3;
    }

    private bool TryPlaceBlockAnywhere(bool[,] grid, BlockShape block)
    {
        List<Vector2Int> positions = GetBaseValidPlacements(block);
        foreach (var pos in positions)
        {
            if (CanPlaceBlockInTest(grid, pos.x, pos.y, block))
            {
                PlaceBlockInTest(grid, pos.x, pos.y, block);
                //Debug.Log($"Successfully placed block at x={pos.x}, y={pos.y}: width={block.width}, height={block.height}, count={block.count}");
                return true;
            }
        }
        //Debug.Log($"Failed to place block anywhere: width={block.width}, height={block.height}, count={block.count}");
        return false;
    }

    private bool HasValidPlacementInTest(bool[,] grid, BlockShape block)
    {
        List<Vector2Int> positions = GetBaseValidPlacements(block);
        foreach (var pos in positions)
        {
            if (CanPlaceBlockInTest(grid, pos.x, pos.y, block))
                return true;
        }
        //Debug.Log($"No valid placement found for block: width={block.width}, height={block.height}, count={block.count}");
        return false;
    }

    private BlockShape SelectWeightedRandomBlock(List<BlockShape> validBlocks)
    {
        float totalWeight = 0;
        foreach (var block in validBlocks)
        {
            totalWeight += Mathf.Pow(random_weight, block.count - 1);
        }

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentSum = 0;

        foreach (var block in validBlocks)
        {
            currentSum += Mathf.Pow(random_weight, block.count - 1);
            if (randomValue <= currentSum)
            {
                return block;
            }
        }

        return validBlocks[validBlocks.Count - 1];
    }

    private bool[,] CloneGrid(bool[,] original)
    {
        bool[,] clone = new bool[original.GetLength(0), original.GetLength(1)];
        for (int x = 0; x < original.GetLength(0); x++)
            for (int y = 0; y < original.GetLength(1); y++)
                clone[x, y] = original[x, y];
        return clone;
    }

    private bool CheckAndClearLines(bool[,] grid)
    {
        bool clearedAnyLine = false;

        // 가로줄 확인
        for (int y = 0; y < gridManager.gridHeight; y++)
        {
            bool isLineFull = true;
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                if (!grid[x, y])
                {
                    isLineFull = false;
                    break;
                }
            }

            if (isLineFull)
            {
                clearedAnyLine = true;
                for (int x = 0; x < gridManager.gridWidth; x++)
                {
                    grid[x, y] = false;
                }
            }
        }

        // 세로줄 확인
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            bool isLineFull = true;
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                if (!grid[x, y])
                {
                    isLineFull = false;
                    break;
                }
            }

            if (isLineFull)
            {
                clearedAnyLine = true;
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    grid[x, y] = false;
                }
            }
        }

        return clearedAnyLine;
    }

    private bool CanPlaceBlockInTest(bool[,] testGrid, int x, int y, BlockShape blockShape)
    {
        if (blockShape == null || testGrid == null) return false;
        if (x < 0 || y < 0 || x + blockShape.width > testGrid.GetLength(0) ||
            y + blockShape.height > testGrid.GetLength(1))
            return false;

        for (int i = 0; i < blockShape.width; i++)
        {
            for (int j = 0; j < blockShape.height; j++)
            {
                if (blockShape.shape[j, i] == 1 && testGrid[x + i, y + j])
                    return false;
            }
        }
        return true;
    }

    private void PlaceBlockInTest(bool[,] testGrid, int x, int y, BlockShape blockShape)
    {
        if (testGrid == null || blockShape == null ||
            x < 0 || y < 0 ||
            x + blockShape.width > testGrid.GetLength(0) ||
            y + blockShape.height > testGrid.GetLength(1))
            return;

        for (int i = 0; i < blockShape.width; i++)
            for (int j = 0; j < blockShape.height; j++)
                if (blockShape.shape[j, i] == 1)
                    testGrid[x + i, y + j] = true;
    }

    private List<Vector2Int> GetBaseValidPlacements(BlockShape block)
    {
        if (baseValidPlacementsCache.TryGetValue(block, out List<Vector2Int> cache))
        {
            return cache;
        }
        List<Vector2Int> validPositions = new List<Vector2Int>();
        // 빈 그리드에서 블록을 놓을 수 있는 모든 좌표 후보 계산 (그리드 크기는 gridManager 기준)
        for (int x = 0; x <= gridManager.gridWidth - block.width; x++)
        {
            for (int y = 0; y <= gridManager.gridHeight - block.height; y++)
            {
                validPositions.Add(new Vector2Int(x, y));
            }
        }
        baseValidPlacementsCache[block] = validPositions;
        return validPositions;
    }

    public void RemoveCurrentBlock(GameObject block)
    {
        if (currentBlocks == null)
            return;

        for (int i = 0; i < currentBlocks.Length; i++)
        {
            if (currentBlocks[i] == block)
            {
                currentBlocks[i] = null;
                return;
            }
        }
    }
}
