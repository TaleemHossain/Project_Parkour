using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenu : MonoBehaviour
{
    public void PlayGame(int n)
    {
        FindFirstObjectByType<AudioManager>().PlaySound("Click");
        SceneManager.LoadScene(n);
    }
}
