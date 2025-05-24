using UnityEngine;
using UnityEngine.SceneManagement;

public class levelMenu : MonoBehaviour
{
    public void PlayGame(int n)
    {
        SceneManager.LoadScene(n);
    }
}
