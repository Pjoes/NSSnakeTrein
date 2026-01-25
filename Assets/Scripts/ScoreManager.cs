using UnityEngine;
using System.IO;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Score")]
    public int score;
    private int highScore;
    private bool highScoreLoaded = false;

    [Header("Persistence")]
    [SerializeField] private string highScoreFileName = "highscore.txt";

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI currentScoreText, finalScoreText;

    private void Start()
    {
        highScore = LoadHighScore();
        highScoreLoaded = true;
        UpdateCurrentScore();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateCurrentScore();
    }

    public void UpdateCurrentScore()
    {
        // Ensure high score is available before updating display
        if (!highScoreLoaded)
        {
            highScore = LoadHighScore();
            highScoreLoaded = true;
        }

        // Update high score immediately when surpassed
        if (score > highScore)
        {
            highScore = score;
            SaveHighScore(highScore);
        }

        // Display both score and highscore
        if (currentScoreText != null)
        {
            currentScoreText.text = $"Score: {score}   Highscore: {highScore}";
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

    public void ManageFinalScore()
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {score}\nHigh Score: {highScore}";
            HideCurrentScore();
        }
        else
        {
            Debug.LogWarning("Score Display Text (TMP_Text) is not assigned in ScoreManager!");
        }

        // Save new high score only if it's higher than current
        int existingHigh = LoadHighScore();
        if (score > existingHigh)
        {
            SaveHighScore(score);
            highScore = score;
        }
    }

    private string GetHighScorePath()
    {
        return Path.Combine(Application.persistentDataPath, highScoreFileName);
    }

    private int LoadHighScore()
    {
        string path = GetHighScorePath();
        if (File.Exists(path))
        {
            try
            {
                string contents = File.ReadAllText(path);
                if (int.TryParse(contents, out int value))
                {
                    return value;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to read high score: {e.Message}");
            }
        }
        return 0;
    }

    private void SaveHighScore(int value)
    {
        string path = GetHighScorePath();
        try
        {
            File.WriteAllText(path, value.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to write high score: {e.Message}");
        }
    }

    private void CheckForDifficultyUpdate()
    {
        DifficultyManager difficultyManager = FindFirstObjectByType<DifficultyManager>();
        if (difficultyManager != null && score != 0)
        {
            difficultyManager.IncreaseDifficulty(score);
        }
    }
}
