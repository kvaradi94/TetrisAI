using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public TextMeshProUGUI scoreText;
    private int score = 0;
    public int CurrentScore => score;
    public int pointForClearingLine = 1000;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        UpdateUI();
    }

    public void AddPoints()
    {
        score += pointForClearingLine;
        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = score.ToString();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateUI();
    }
}
