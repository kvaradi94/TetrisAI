using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TetrisBlock : MonoBehaviour
{

    public Vector3 rotationPoint;
    private float prevTime;
    public float fallTime = 2f;
    public static int height = 28;
    public static int width = 16;
    private static Transform[,] grid = new Transform[height, width];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            transform.position += new Vector3(-1, 0, 0);
            if (!ValidMove())
            {
                transform.position -= new Vector3(-1, 0, 0);
            }
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            transform.position += new Vector3(1, 0, 0);
            if (!ValidMove())
            {
                transform.position -= new Vector3(1, 0, 0);
            }
        }
        else if (Time.time - prevTime > (Keyboard.current.downArrowKey.isPressed ? fallTime / 10 : fallTime))
        {
            transform.position += new Vector3(0, -1, 0);
            if (!ValidMove())
            {
                transform.position -= new Vector3(0, -1, 0);
                AddToGrid();
                CheckForLines();
                this.enabled = false;
                FindObjectOfType<SpawnTetromino>().NewTetromino();
            }
            prevTime = Time.time;
        }
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
            if (!ValidMove())
            {
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
            }
        }
    }

    void CheckForLines()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            if (HasLine(y))
            {
                DeleteLine(y);
                RowDown(y + 1);
                y++; // recheck same row after shifting
            }
        }
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


    void AddToGrid()
    {
        foreach (Transform children in transform)
        {
            int x = Mathf.RoundToInt(children.transform.position.x);
            int y = Mathf.RoundToInt(children.transform.position.y);

            x -= 8;
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
            x -= 8;
            y -= 1;

            Debug.Log(x);
            Debug.Log(y);

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
}
