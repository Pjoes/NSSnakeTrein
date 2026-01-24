using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public int score;
    [SerializeField] private TextMeshProUGUI scoreDisplayText;

    public void DisplayFinalScore()
    {
        if (scoreDisplayText != null)
        {
            scoreDisplayText.text = "Final Score: " + score;
        }
        else
        {
            Debug.LogWarning("Score Display Text (TMP_Text) is not assigned in ScoreManager!");
        }
    }
}
