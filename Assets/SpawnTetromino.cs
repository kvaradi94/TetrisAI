using UnityEngine;

public class SpawnTetromino : MonoBehaviour
{

    public GameObject[] Tetrominos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NewTetromino();
    }

    // This is for human player
    // public void NewTetromino()
    // {
    //     Instantiate(Tetrominos[Random.Range(0, Tetrominos.Length)], transform.position, Quaternion.identity); 
    // }

    // This is for AI agent
    public void NewTetromino()
    {
        GameObject obj = Instantiate(
            Tetrominos[Random.Range(0, Tetrominos.Length)],
            transform.position,
            Quaternion.identity
        );

        TetrisBlock block = obj.GetComponent<TetrisBlock>();

        var agent = FindObjectOfType<PlayTetrisAgent>();
        if (agent != null && block != null)
        {
            agent.SetCurrentPiece(block.Type);
        }
    }
}
