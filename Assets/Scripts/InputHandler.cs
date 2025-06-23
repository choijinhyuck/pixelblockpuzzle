using UnityEngine;
using System.Collections.Generic; // added for List<>

public class InputHandler : MonoBehaviour
{
    // 추가: 드래그 활성화 여부 플래그
    private bool isDragEnabled = true;
    
    private GameObject selectedBlock;
    private GameObject ghostBlock;
    private Vector3 offset;
    private GridManager gridManager;
    private BlockManager blockManager;
    private ScoreManager scoreManager;
    private List<GameObject> flashingCells = new List<GameObject>(); // new field
    private (int x, int y) lastClearPosition; // new field
    public GameObject floatingScoreTextPrefab; // assign the prefab in the Inspector

    void Start()
    {
        gridManager = GameManager.Instance.gridManager;
        blockManager = GameManager.Instance.blockManager;
        scoreManager = GameManager.Instance.scoreManager;
    }

    void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        // 드래그 기능이 비활성화 되었으면 입력을 무시
        if (!isDragEnabled) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Block"))
            {
                selectedBlock = hit.collider.gameObject;
                offset = mousePos - selectedBlock.transform.position;
                offset.x += selectedBlock.GetComponent<BoxCollider2D>().offset.x * 0.5f;
                selectedBlock.transform.position = new Vector3(mousePos.x - offset.x, mousePos.y - offset.y + 2.5f, -1);
                selectedBlock.transform.localScale = Vector3.one;

                CreateGhostBlock(selectedBlock.GetComponent<Block>());
            }
        }

        if (Input.GetMouseButton(0) && selectedBlock != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selectedBlock.transform.position = new Vector3(mousePos.x - offset.x, mousePos.y - offset.y + 2.5f, -1);
            selectedBlock.transform.localScale = Vector3.one;
            
            // Update ghost block position (modified y coordinate calculation)
            Vector3 gridPos = new Vector3(
                Mathf.Round(mousePos.x - offset.x),
                Mathf.Round(mousePos.y - offset.y + 3) - 0.5f,
                2
            );
            ghostBlock.transform.position = gridPos;

            if (gridManager.IsIndexValid(Mathf.RoundToInt(gridPos.x), Mathf.RoundToInt(gridPos.y + 0.5f))
                && gridManager.CanPlaceBlock(Mathf.RoundToInt(gridPos.x), Mathf.RoundToInt(gridPos.y + 0.5f), ghostBlock.GetComponent<Block>().shape))
            {
                ghostBlock.SetActive(true);
            }
            else
            {
                ghostBlock.SetActive(false);
            }

            foreach (GameObject cell in flashingCells)
            {
                if (cell != null)
                {
                    SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.material.SetFloat("_FlashDegree", 0);
                }
            }
            flashingCells.Clear();

            foreach (Transform unit in selectedBlock.transform)
            {
                SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.material.SetFloat("_FlashDegree", 0);
            }

            if (!ghostBlock.activeSelf || ghostBlock == null)
            {
                return;
            }

            // --- New Code: Flash selectedBlock's unit children participating in a potential clear ---
            int simX = Mathf.RoundToInt(gridPos.x);
            int simY = Mathf.RoundToInt(gridPos.y + 0.5f);
            var potentialClears = gridManager.GetPotentialClearedLines(simX, simY, ghostBlock.GetComponent<Block>().shape);

            // Reset flash on previously highlighted grid cells

            
            // Flash grid cells as before (if you still want them lit)
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    if (potentialClears.clearedRows.Contains(y) || potentialClears.clearedColumns.Contains(x))
                    {
                        GameObject cell = gridManager.grid[x, y];
                        if (cell != null)
                        {
                            SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                            if (sr != null)
                            {
                                sr.material.SetFloat("_FlashDegree", Mathf.PingPong(Time.time * 2, 1));
                                flashingCells.Add(cell);
                            }
                        }
                    }
                }
            }

            // Flash only selectedBlock's children based on ghostBlock's placement position
            Vector3 ghostOrigin = ghostBlock.transform.position;
            foreach (Transform unit in selectedBlock.transform)
            {
                Vector3 targetPos = ghostOrigin + unit.localPosition;
                int posX = Mathf.RoundToInt(targetPos.x);
                int posY = Mathf.RoundToInt(targetPos.y + 0.5f);
                if (potentialClears.clearedRows.Contains(posY) || potentialClears.clearedColumns.Contains(posX))
                {
                    lastClearPosition = (posX, posY);
                    SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.material.SetFloat("_FlashDegree", Mathf.PingPong(Time.time * 2, 1));
                }
                else
                {
                    SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.material.SetFloat("_FlashDegree", 0);
                }
            }
            // --- End new code ---
        }

        if (Input.GetMouseButtonUp(0) && selectedBlock != null)
        {
            ReleaseDrag();
        }
    }

    // 새로 추가한 메서드: 드래그 해제 시 실행할 로직
    private void ReleaseDrag()
    {
        // On Mouse Up와 동일한 처리
        foreach (GameObject cell in flashingCells)
        {
            if (cell != null)
            {
                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.material.SetFloat("_FlashDegree", 0);
            }
        }
        flashingCells.Clear();

        foreach (Transform unit in selectedBlock.transform)
        {
            SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.material.SetFloat("_FlashDegree", 0);
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 gridPos = new Vector3(
            Mathf.Round(mousePos.x - offset.x),
            Mathf.Round(mousePos.y - offset.y + 3),
            0
        );
        
        int gridX = Mathf.RoundToInt(gridPos.x);
        int gridY = Mathf.RoundToInt(gridPos.y);

        Block selectedBlockComponent = selectedBlock.GetComponent<Block>();
        if (selectedBlockComponent == null) return;
        
        BlockShape currentShape = selectedBlockComponent.shape;
        if (currentShape == null) return;

        if (gridManager.CanPlaceBlock(gridX, gridY, currentShape))
        {
            // Place new block units according to the shape
            for (int x = 0; x < currentShape.width; x++)
            {
                for (int y = 0; y < currentShape.height; y++)
                {
                    if (currentShape.shape[y, x] == 1)
                    {
                        Vector3 unitPosition = new Vector3(gridX + x, gridY + y - 0.5f, 0);
                        GameObject blockUnit = ObjectPooler.Instance.SpawnFromPool(blockManager.blockUnitPrefabs[selectedBlockComponent.blockColorIndex], unitPosition, Quaternion.identity);
                        gridManager.SetBlock(gridX + x, gridY + y, blockUnit);
                    }
                }
            }
            GameManager.Instance.soundManager.PlayBlockPlacedSound();
            blockManager.RemoveCurrentBlock(selectedBlock);
            // Destroy the existing block
            selectedBlock.GetComponent<Block>().ReturnToPool();
            Destroy(selectedBlock);

            selectedBlock = null;

            Block ghost = ghostBlock.GetComponent<Block>();
            ghost.ReturnToPool();
            ghostBlock.SetActive(false);

            blockManager.remainBlockCount--;

            // 블록 배치 성공시 점수 추가
            scoreManager.AddBlockScore(currentShape.count);

            if (gridManager.CheckAndClearFullLines())
            {
                int scoreGained = scoreManager.lastClearScore; 
                Vector3 effectPosition = new Vector3(lastClearPosition.x, lastClearPosition.y, 0);
                GameObject floatingScoreText = ObjectPooler.Instance.SpawnFromPool(floatingScoreTextPrefab, effectPosition, Quaternion.identity);
                floatingScoreText.transform.SetParent(GameManager.Instance.uiManager.canvas.transform);
                floatingScoreText.GetComponent<FloatingScoreText>().ShowScore(scoreGained.ToString(), effectPosition, GameManager.Instance.uiManager.canvas);

                VibrateManager.Instance.TriggerStrongVibration();
            }
            else
            {
                VibrateManager.Instance.TriggerWeakVibration();
            }

            scoreManager.ResetCombo();
            if (GameManager.Instance.CheckGameClear())
            {
                Debug.Log("Game Cleared!");
                return;
            }

            if (blockManager.remainBlockCount > 0)
            {
                if (GameManager.Instance.CheckGameOver())
                {
                    Debug.Log("No valid moves remaining!");
                }
                else
                {
                    Debug.Log("valid moves remaining");
                }
            }

            if (blockManager.remainBlockCount <= 0)
            {
                blockManager.GenerateNewBlocks();
            }                
        }
        else
        {
            // Return block to original position
            selectedBlock.GetComponent<Block>().ResetPositionAndScale();
            selectedBlock = null;
            Block ghost = ghostBlock.GetComponent<Block>();
            ghost.ReturnToPool();
            ghostBlock.SetActive(false);
        }
    }

    // 드래그 기능을 비활성화하는 메서드
    public void DisableDrag()
    {
        isDragEnabled = false;
    }

    public void EnableDrag()
    {
        isDragEnabled = true;
    }
    
    private void CreateGhostBlock(Block selectedBlock)
    {
        if (ghostBlock == null)
        {
            ghostBlock = new GameObject("GhostBlock");
            ghostBlock.AddComponent<Block>();
        }

        Block block = ghostBlock.GetComponent<Block>();
        block.blockColorIndex = selectedBlock.blockColorIndex;
        block.Initialize(selectedBlock.shape);
        
        // Add a sprite renderer for visual representation
        foreach (Transform child in ghostBlock.transform)
        {
            child.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.7f);
        }
        
        ghostBlock.SetActive(false);
    }

    // 앱이 Pause 상태로 전환될 때 호출 (예: 다른 화면 전환, 홈 버튼 등)
    void OnApplicationPause(bool pause)
    {
        if (pause && selectedBlock != null)
        {
            ReleaseDrag();
        }
    }
}
