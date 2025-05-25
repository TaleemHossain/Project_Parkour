using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused = false;
    public GameObject player;
    PlayerController playerController;
    public GameObject pauseMenuUI;
    void Awake()
    {
        playerController = player.GetComponent<PlayerController>();
    }
    void Update()
    {
        if (player == null) return;
        if (playerController.CanPause)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsPaused)
                {
                    FindFirstObjectByType<AudioManager>().PlaySound("Click");
                    Resume();
                }
                else
                {
                    FindFirstObjectByType<AudioManager>().PlaySound("Click");
                    Pause();
                }
            }
        }
    }
    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
