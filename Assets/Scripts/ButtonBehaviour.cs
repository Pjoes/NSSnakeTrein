using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonBehaviour : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "GameLevel";

    [Header("Panels")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private GameObject mainMenuPanel;

    public void StartGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void RestartGame()
    {
        // Just reload the current scene since there's only one gameplay scene right now
        // TODO: Extend if more scenes are added
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
        }
    }

    public void HideInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
