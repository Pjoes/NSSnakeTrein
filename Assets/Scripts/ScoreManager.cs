using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Score")]
    public int score;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI currentScoreText, finalScoreText;
    public void AddScore(int amount)
    {
        score += amount;
        UpdateCurrentScore();
    }

    public void UpdateCurrentScore()
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = score.ToString();
        }
        else
        {
            Debug.LogWarning("Current Score Text (TMP_Text) is not assigned in ScoreManager!");
        }

        CheckForDifficultyUpdate();
    }

    public void HideCurrentScore()
    {
        currentScoreText.gameObject.SetActive(false);
    }

    public void DisplayFinalScore()
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + score;
            HideCurrentScore();
        }
        else
        {
            Debug.LogWarning("Score Display Text (TMP_Text) is not assigned in ScoreManager!");
        }
    }

    private void CheckForDifficultyUpdate()
    {
        DifficultyManager difficultyManager = FindFirstObjectByType<DifficultyManager>();
        if (difficultyManager != null)
        {
            difficultyManager.IncreaseDifficulty(score);
        }
    }
}
