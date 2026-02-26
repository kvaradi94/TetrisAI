using Unity.MLAgents;
using UnityEngine;
using UnityEngine.InputSystem;

public class TetrisBlock : MonoBehaviour
{

    public Vector3 rotationPoint;
    private float prevTime;
    public float fallTime = 0.02f;
    public static int height = 24;
    public static int width = 10;
    public static Transform[,] grid = new Transform[height, width];
    public bool IsPlaced { get; private set; }
    public TetrominoType Type;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IsPlaced = false;
        prevTime = Time.time;
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        // if (Time.time - prevTime > (Keyboard.current.downArrowKey.isPressed ? fallTime / 20 : fallTime / 2))
        // {
        //     transform.position += new Vector3(0, -1, 0);
        //     if (!ValidMove())
        //     {
        //         transform.position -= new Vector3(0, -1, 0);
        //         AddToGrid();
        //         // CheckForLines();
        //         int linesCleared = CheckForLines();
        //         this.enabled = false;
        //         FindObjectOfType<SpawnTetromino>().NewTetromino();

        //         if (GetCurrentGridHeight() > height - 6)
        //         {
        //             // GameOverHandler.GetInstance()?.DisplayGameOver();
        //             // Time.timeScale = 0f;
        //             GameOverHandler.GetInstance()?.DisplayGameOver();
        //             return;
        //         }
        //     }
        //     prevTime = Time.time;
        // }

        // if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        // {
        //     transform.position += new Vector3(-1, 0, 0);
        //     if (!ValidMove())
        //     {
        //         transform.position -= new Vector3(-1, 0, 0);
        //     }
        // }
        // else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        // {
        //     transform.position += new Vector3(1, 0, 0);
        //     if (!ValidMove())
        //     {
        //         transform.position -= new Vector3(1, 0, 0);
        //     }
        // }
        // else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        // {
        //     transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
        //     if (!ValidMove())
        //     {
        //         transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
        //     }
        // }
        // Debug.Log("maxheight: " + GetMaxHeight());
        // Debug.Log("hole: " + CountHoles());
    }

    public int CheckForLines()
    {
        int linesCleared = 0;

        for (int y = height - 1; y >= 0; y--)
        {
            if (HasLine(y))
            {
                DeleteLine(y);
                RowDown(y + 1);
                ScoreManager.Instance.AddPoints();
                linesCleared++;
                y++;
            }
        }

        return linesCleared;
    }

    bool HasLine(int i)
    {
        for (int j = 0; j < width; j++)
        {
            if (grid[i, j] == null)
            {
                return false;
            }
        }
        return true;
    }

    void DeleteLine(int i)
    {
        for (int j = 0; j < width; j++)
        {
            Destroy(grid[i, j].gameObject);
            grid[i, j] = null;
        }
    }

    void RowDown(int startY)
    {
        for (int y = startY; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[y, x] != null)
                {
                    grid[y - 1, x] = grid[y, x];
                    grid[y, x] = null;
                    grid[y - 1, x].transform.position += Vector3.down;
                }
            }
        }
    }


    public void AddToGrid()
    {
        IsPlaced = true;
        foreach (Transform children in transform)
        {
            int x = Mathf.RoundToInt(children.transform.position.x);
            int y = Mathf.RoundToInt(children.transform.position.y);

            x -= 11;
            y -= 1;

            grid[y, x] = children;
        }
    }

    bool ValidMove()
    {
        foreach (Transform children in transform)
        {
            int x = Mathf.RoundToInt(children.transform.position.x);
            int y = Mathf.RoundToInt(children.transform.position.y);

            //Offset the x and y to the GameArea position
            x -= 11;
            y -= 1;

            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return false;
            }

            if (grid[y, x] != null)
            {
                return false;
            }
        }

        return true;
    }

    int GetCurrentGridHeight()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[y, x] != null)
                {
                    return y + 1; // +1 makes it human-readable (height, not index)
                }
            }
        }
        return 0;
    }

    // For the agent
    public bool RotateOnce()
    {
        transform.RotateAround(transform.TransformPoint(rotationPoint),
            new Vector3(0, 0, 1), 90);

        if (!ValidMove())
        {
            transform.RotateAround(transform.TransformPoint(rotationPoint),
                new Vector3(0, 0, 1), -90);
            return false;
        }

        return true;
    }

    public bool MoveHorizontal(int dir)
    {
        transform.position += new Vector3(dir, 0, 0);

        if (!ValidMove())
        {
            transform.position -= new Vector3(dir, 0, 0);
            return false;
        }

        return true;
    }

    public void MoveToColumn(int targetColumn)
    {
        // Convert grid column (0–15) to world X
        int worldTargetX = targetColumn + 8;

        int safety = 50; // avoid infinite loops

        while (Mathf.RoundToInt(transform.position.x) != worldTargetX && safety-- > 0)
        {
            int dir = transform.position.x < worldTargetX ? 1 : -1;

            if (!MoveHorizontal(dir))
                break;
        }
    }

    public static void ClearGrid()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[y, x] != null)
                {
                    Destroy(grid[y, x].gameObject);
                    grid[y, x] = null;
                }
            }
        }
    }

    public static int CountHoles()
    {
        int holes = 0;

        for (int x = 0; x < width; x++)
        {
            bool foundBlock = false;

            for (int y = height - 1; y >= 0; y--)
            {
                if (grid[y, x] != null)
                {
                    foundBlock = true;
                }
                else if (foundBlock)
                {
                    holes++;
                }
            }
        }

        return holes;
    }

    public static int GetMaxHeight()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[y, x] != null)
                    return y + 1;
            }
        }
        return 0;
    }

    public static int[] GetColumnHeights()
    {
        int[] heights = new int[width];

        for (int x = 0; x < width; x++)
        {
            heights[x] = 0;

            for (int y = height - 1; y >= 0; y--)
            {
                if (grid[y, x] != null)
                {
                    heights[x] = y + 1;
                    break;
                }
            }
        }

        return heights;
    }

    public void HardDrop()
    {
        while (true)
        {
            transform.position += Vector3.down;

            if (!ValidMove())
            {
                transform.position -= Vector3.down;
                break;
            }
        }
    }
}
