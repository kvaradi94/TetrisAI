using UnityEngine;
using TMPro;

public class GameOverHandler : MonoBehaviour
{

    public static GameOverHandler Instance;
    public TextMeshProUGUI gameOverText;
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
    }

    public static GameOverHandler GetInstance()
    {
        if (Instance == null)
        {
            Instance = FindObjectOfType<GameOverHandler>();
        }
        return Instance;
    }

    public void DisplayGameOver()
    {
        if (gameOverText != null)
        {
            gameOverText.text = "Game Over";
        }
    }

    public void ResetGameOver()
    {
        if (gameOverText != null)
            gameOverText.text = "";
    }
}
