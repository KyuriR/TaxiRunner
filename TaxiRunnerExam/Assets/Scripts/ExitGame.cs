using UnityEngine;
using UnityEngine.InputSystem;

public class ExitGame : MonoBehaviour
{
    public void QuitGame()
    {
       

        Application.Quit();

        
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) 
        {
            QuitGame();
        }
    }
}
