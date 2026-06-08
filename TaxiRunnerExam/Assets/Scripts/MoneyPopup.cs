using UnityEngine;
using TMPro;

public class MoneyPopup : MonoBehaviour
{
    public float floatSpeed = 100f;
    public float lifeTime = 1f;

    private TextMeshProUGUI text;
    private RectTransform rect;
    private float timer;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();
    }

    public void SetAmount(int amount)
    {
        if (text != null)
            text.text = "+R" + amount;
    }

    public void SetCustomText(string value)
    {
        if (text != null)
            text.text = value;
    }

    void Update()
    {
        rect.anchoredPosition += Vector2.up * floatSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        if (text != null)
        {
            float t = timer / lifeTime;
            Color c = text.color;
            c.a = 1f - t;
            text.color = c;
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }
}