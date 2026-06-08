using UnityEngine;
using UnityEngine.UI;

public class WheelLives : MonoBehaviour
{
    public Image[] wheels;

    void Start()
    {
        UpdateWheels();
    }

    void Update()
    {
        UpdateWheels();
    }

    void UpdateWheels()
    {
        if (GameManager.Instance == null) return;

        int lives = GameManager.Instance.wheels;

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] != null)
                wheels[i].enabled = i < lives;
        }
    }
}