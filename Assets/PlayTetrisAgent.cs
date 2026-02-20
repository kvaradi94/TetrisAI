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

    public override void OnActionReceived(ActionBuffers actions)
    {
        var block = FindObjectOfType<TetrisBlock>();
        if (block == null || block.IsPlaced)
            return;

        int targetColumn = actions.DiscreteActions[0]; // 0–15
        int rotation = actions.DiscreteActions[1];     // 0–3

        // Apply rotation
        for (int i = 0; i < rotation; i++)
            block.RotateOnce();

        // Move horizontally
        block.MoveToColumn(targetColumn);

        // INSTANT DROP
        block.HardDrop();

        // Lock piece
        block.AddToGrid();

        int linesCleared = block.CheckForLines();

        var agent = this;

        bool gameOver = TetrisBlock.GetMaxHeight() > TetrisBlock.height - 6;

        if (gameOver)
        {
            GameOverHandler.GetInstance()?.DisplayGameOver();
            agent.GameOver();
            return;
        }
        else
        {
            agent.EvaluatePlacement(linesCleared);
        }

        FindObjectOfType<SpawnTetromino>()?.NewTetromino();
    }

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
            float reward = -0.1f * currentHeight;
            AddReward(reward); 
            previousHeight = currentHeight;
        }

        if (currentHoles > previousHoles)
        {
            float reward = -0.3f * currentHeight;
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
