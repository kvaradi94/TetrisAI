using UnityEngine;

public class SpawnTetromino : MonoBehaviour
{

    public GameObject[] Tetrominos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NewTetromino();
    }

    // public void NewTetromino()
    // {
    //     Instantiate(Tetrominos[Random.Range(0, Tetrominos.Length)], transform.position, Quaternion.identity); 
    // }

    public void NewTetromino()
    {
        Instantiate(
            Tetrominos[Random.Range(0, Tetrominos.Length)],
            transform.position,
            Quaternion.identity
        );

        // 🔔 Tell the agent to act NOW
        var agent = FindObjectOfType<PlayTetrisAgent>();
        if (agent != null)
        {
            agent.RequestDecision();
        }
    }
}
