using System;
using TMPro;
using UnityEngine;

public class GameOverController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public static Action OnBackToMap;

    public void SetText(string text, Color color)
    {
        this.text.text = text;
        this.text.color = color;
    }

    public void OnClick()
    {
        OnBackToMap?.Invoke();
    }
}
