using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public void PlayClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButton();
        }
    }

    public void PlayClose()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClose();
        }
    }
}