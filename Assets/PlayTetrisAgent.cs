using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayTetrisAgent : Agent
{
    private int previousHeight;
    private int previousHoles;
    private TetrominoType currentPieceType;
    private float rewards = 0f;
    private int scores = 0;
    private int maxScore = 0;
    private int maxScoreEpisode = 0;

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
        float reward = 500f * linesCleared;
        int currentHeight = TetrisBlock.GetMaxHeight();
        int currentHoles = TetrisBlock.CountHoles();

        if (currentHeight > previousHeight)
        {
            reward += -3f * (currentHeight - previousHeight);
        }

        if (currentHoles > previousHoles)
        {
            reward += -10f * (currentHoles - previousHoles);
        }
        previousHeight = currentHeight;
        previousHoles = currentHoles;
        AddReward(reward);
    }
    public void GameOver()
    {
        AddReward(-1000f);

        rewards += GetCumulativeReward();
        scores += ScoreManager.Instance.CurrentScore; 

        if (ScoreManager.Instance.CurrentScore > maxScore)
        {
            maxScore = ScoreManager.Instance.CurrentScore;
            maxScoreEpisode = CompletedEpisodes;
        }

        if (CompletedEpisodes % 100 == 0)
        {
            Debug.Log($"Episodes: {CompletedEpisodes}, AVG Reward: {rewards / 10:F1}, AVG score: {scores / 10:F1}, MAX Score: {maxScore:F1}, MAX Score Episode: {maxScoreEpisode}");
            rewards = 0;
            scores = 0;
        }

        EndEpisode();
    }
}
