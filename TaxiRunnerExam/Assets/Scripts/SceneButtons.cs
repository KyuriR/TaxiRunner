using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtons : MonoBehaviour
{
    public void LoadStart()
    {
    
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }

    public void LoadGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }
}