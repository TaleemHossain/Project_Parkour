using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    public void PlayGame()
    {
        FindFirstObjectByType<AudioManager>().PlaySound("Click");
        SceneManager.LoadScene(1);

    }
    public void QuitGame()
    {
        FindFirstObjectByType<AudioManager>().PlaySound("Click");
        Application.Quit();
    }
    public void ClickSound()
    {
        FindFirstObjectByType<AudioManager>().PlaySound("Click");
    }
}
