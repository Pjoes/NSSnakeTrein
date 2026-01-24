using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonBehaviour : MonoBehaviour
{
    public void Restart()
    {
        // Just reload the current scene since there's only one gameplay scene right now
        // TODO: Extend if more scenes are added
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void MainMenu()
    {

    }
}
