using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitAssignController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amountText;

    [SerializeField] private Image icon;

    public Button addButton;

    private Unit unit;

    int amount = 0;

    int maxAmount = 0;

    public static Action<int, Unit> UnitAmountChanged;

    public void Setup(int max, Sprite icon, Unit unit)
    {
        SetMaxAmount(max);
        SetImage(icon);
        SetUnit(unit);
        UpdateString();
    }

    public void Increase()
    {
        if (amount < maxAmount)
        {
            amount++;
            UpdateString();
        }
    }

    public void Decrease()
    {
        if (amount > 0)
        {
            amount--;
            UpdateString();
        }
    }

    private void UpdateString()
    {
        amountText.text = $"{amount} / {maxAmount}";

        UnitAmountChanged?.Invoke(amount, unit);
    }

    public void SetMaxAmount(int max)
        => maxAmount = max;

    public int GetAmount()
        => amount;

    public void SetImage(Sprite sprite)
        => icon.sprite = sprite;

    public void SetUnit(Unit unit)
        => this.unit = unit;
}
