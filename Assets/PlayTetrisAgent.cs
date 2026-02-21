using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayTetrisAgent : Agent
{
    private int previousHeight;
    private int previousHoles;
    private TetrominoType currentPieceType;

    public void SetCurrentPiece(TetrominoType type)
    {
        currentPieceType = type;
    }

    public override void OnEpisodeBegin()
    {
        TetrisBlock.ClearGrid();

        foreach (var block in FindObjectsOfType<TetrisBlock>())
            Destroy(block.gameObject);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();

        GameOverHandler.GetInstance()?.ResetGameOver();

        FindObjectOfType<SpawnTetromino>()?.NewTetromino();

        previousHeight = 0;
        previousHoles = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        for (int y = 0; y < TetrisBlock.height; y++)
        {
            for (int x = 0; x < TetrisBlock.width; x++)
            {
                sensor.AddObservation(TetrisBlock.grid[y, x] != null ? 1f : 0f);
            }
        }

        float[] oneHot = new float[7];
        oneHot[(int)currentPieceType] = 1f;

        foreach (float v in oneHot)
            sensor.AddObservation(v);
    }

    // this is for the AI agent that actually plays the tetris
    public void OnPiecePlaced(int linesCleared)
    {
        if (linesCleared > 0)
            AddReward(5f * linesCleared);

        int height = TetrisBlock.GetMaxHeight();
        int holes = TetrisBlock.CountHoles();

        AddReward(-0.01f * height);
        AddReward(-0.02f * holes);

        if (height > TetrisBlock.height - 6)
        {
            AddReward(-10f);
            EndEpisode();
        }
    }

    // this is for the AI agent that actually plays the tetris
    private void FixedUpdate()
    {
        RequestDecision();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var block = FindObjectOfType<TetrisBlock>();
        if (block == null || block.IsPlaced)
            return;

        int action = actions.DiscreteActions[0];

        switch (action)
        {
            case 1:
                block.MoveHorizontal(-1);
                AddReward(-0.01f);
                break;

            case 2:
                block.MoveHorizontal(1);
                AddReward(-0.01f);
                break;

            case 3:
                block.RotateOnce();
                AddReward(-0.01f);
                break;

            case 0:
            default:
                break;
        }

        bool gameOver = TetrisBlock.GetMaxHeight() > TetrisBlock.height - 6;

        if (gameOver)
        {
            GameOverHandler.GetInstance()?.DisplayGameOver();
            this.GameOver();
            return;
        }
        else
        {
            int linesCleared = block.CheckForLines();
            this.EvaluatePlacement(linesCleared);
        }

        // AddReward(-0.001f); // small time penalty to encourage faster play
    }

    // This is for one decision per piece 
    // public override void OnActionReceived(ActionBuffers actions)
    // {
    //     var block = FindObjectOfType<TetrisBlock>();
    //     if (block == null || block.IsPlaced)
    //         return;

    //     int targetColumn = actions.DiscreteActions[0]; // 0–15
    //     int rotation = actions.DiscreteActions[1];     // 0–3

    //     // Apply rotation
    //     for (int i = 0; i < rotation; i++)
    //         block.RotateOnce();

    //     // Move horizontally
    //     block.MoveToColumn(targetColumn);

    //     // INSTANT DROP
    //     block.HardDrop();

    //     // Lock piece
    //     block.AddToGrid();

    //     int linesCleared = block.CheckForLines();

    //     var agent = this;

    //     bool gameOver = TetrisBlock.GetMaxHeight() > TetrisBlock.height - 6;

    //     if (gameOver)
    //     {
    //         GameOverHandler.GetInstance()?.DisplayGameOver();
    //         agent.GameOver();
    //         return;
    //     }
    //     else
    //     {
    //         agent.EvaluatePlacement(linesCleared);
    //     }

    //     FindObjectOfType<SpawnTetromino>()?.NewTetromino();
    // }

    public void EvaluatePlacement(int linesCleared)
    {
        if (linesCleared > 0)
        {
            AddReward(1000f);
            return;
        }

        int currentHeight = TetrisBlock.GetMaxHeight();
        int currentHoles = TetrisBlock.CountHoles();

        if (currentHeight > previousHeight)
        {
            float reward = -1f * currentHeight;
            AddReward(reward);
            previousHeight = currentHeight;
        }

        if (currentHoles > previousHoles)
        {
            float reward = -5f * currentHeight;
            AddReward(reward);
            previousHoles = currentHoles;
        }

    }
    public void GameOver()
    {
        AddReward(-1000f);

        Debug.Log($"Episodes: {CompletedEpisodes}, Total Reward: {GetCumulativeReward():F3}, Point scored: {ScoreManager.Instance.CurrentScore}");

        EndEpisode();
    }
}
