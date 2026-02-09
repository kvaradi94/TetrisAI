using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayTetrisAgent : Agent
{
    private int lastScore;

    public override void OnEpisodeBegin()
    {
        TetrisBlock.ClearGrid();

        foreach (var block in FindObjectsOfType<TetrisBlock>())
            Destroy(block.gameObject);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();

        GameOverHandler.GetInstance()?.ResetGameOver();

        FindObjectOfType<SpawnTetromino>()?.NewTetromino();

        lastScore = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(
            ScoreManager.Instance != null
                ? ScoreManager.Instance.CurrentScore / 1000f
                : 0f
        );
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var block = FindObjectOfType<TetrisBlock>();
        if (block == null)
            return;

        // Prevent multiple decisions for the same tetromino
        if (block.IsPlaced)
            return;

        int targetColumn = actions.DiscreteActions[0]; // 0–15
        int rotation = actions.DiscreteActions[1];     // 0–3

        // 1️⃣ Rotate
        for (int i = 0; i < rotation; i++)
            block.RotateOnce();

        // 2️⃣ Move horizontally
        block.MoveToColumn(targetColumn);

        GiveScoreReward();
    }

    void GiveScoreReward()
    {
        if (ScoreManager.Instance == null) return;

        int score = ScoreManager.Instance.CurrentScore;
        int delta = score - lastScore;

        if (delta > 0)
            AddReward(delta * 0.01f);

        lastScore = score;
    }

    public void GameOver()
    {
        AddReward(-1f);
        EndEpisode();
    }
}
