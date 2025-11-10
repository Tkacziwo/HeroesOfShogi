using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssignPanelsController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI limitText;

    [SerializeField] private int unitSizeLimit = 30;

    public Button recruitButton;

    private int currentSize = 0;

    private PlayerCharacterController currentCharacter;

    private List<Unit> tempUnits = new();

    private List<Unit> units = new();

    public static Action<ProducedUnits> OnUnitsRecruit;

    [SerializeField] private GameObject pawnPanel;

    [SerializeField] private GameObject lancePanel;

    [SerializeField] private GameObject horsePanel;

    [SerializeField] private GameObject silverGeneralPanel;

    [SerializeField] private GameObject goldGeneralPanel;

    [SerializeField] private GameObject rookPanel;

    [SerializeField] private GameObject bishopPanel;

    [SerializeField] private Sprite tempSprite;

    private ProducedUnits selectedUnits = new();

    private ProducedUnits cityUnits = new();

    private Unit pawnTemp = new();

    private Unit lanceTemp = new();

    private Unit horseTemp = new();

    public void Setup(PlayerCharacterController currentCharacter, ProducedUnits cityUnits)
    {
        this.currentCharacter = currentCharacter;
        this.cityUnits = cityUnits;
        units = this.currentCharacter.GetAssignedUnits();
        tempUnits = new(units);

        currentSize = CountCurrentSize(tempUnits);
        UpdateString();
        InitPanels();
    }

    private void UpdateString()
    {
        limitText.text = $"Limit: {currentSize} / {unitSizeLimit}";
    }

    private void InitPanels()
    {
        pawnTemp = new Unit()
        {
            HealthPoints = 1,
            AttackPower = 1,
            SizeInArmy = 1,
            UnitName = UnitEnum.Pawn,
            pieceName = "Pawn",
            UnitSprite = StaticData.unitIcons.SingleOrDefault(o => o.name == UnitEnum.Pawn.ToString())
        };

        lanceTemp = new()
        {
            HealthPoints = 1,
            AttackPower = 2,
            SizeInArmy = 1,
            UnitName = UnitEnum.Lance,
            pieceName = "Lance"
        };

        horseTemp = new()
        {
            HealthPoints = 2,
            AttackPower = 2,
            SizeInArmy = 1,
            UnitName = UnitEnum.Horse,
            pieceName = "Horse"
        };

        pawnPanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.pawns, tempSprite, pawnTemp);

        lancePanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.lances, tempSprite, lanceTemp);

        horsePanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.horses, tempSprite, horseTemp);
    }

    public void OnClick()
    {
        if (!recruitButton.interactable) return;

        currentCharacter.SetUnits(tempUnits);

        cityUnits.pawns -= selectedUnits.pawns;
        cityUnits.lances -= selectedUnits.lances;
        cityUnits.horses -= selectedUnits.horses;

        OnUnitsRecruit?.Invoke(cityUnits);

        OnCancel();
    }

    public void OnCancel()
    {
        this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    private void HandleUnitAmountChanged(int amount, Unit unit)
    {
        tempUnits = new(units);
        if (unit.UnitName == UnitEnum.Pawn)
        {
            selectedUnits.pawns = amount;
        }
        else if (unit.UnitName == UnitEnum.Lance)
        {
            selectedUnits.lances = amount;
        }
        else if (unit.UnitName == UnitEnum.Horse)
        {
            selectedUnits.horses = amount;
        }

        for (int i = 0; i < selectedUnits.pawns; i++)
        {
            tempUnits.Add(pawnTemp);
        }
        for (int i = 0; i < selectedUnits.lances; i++)
        {
            tempUnits.Add(lanceTemp);
        }
        for (int i = 0; i < selectedUnits.horses; i++)
        {
            tempUnits.Add(horseTemp);
        }


        currentSize = CountCurrentSize(tempUnits);

        UpdateString();

        if (currentSize >= unitSizeLimit)
        {
            recruitButton.interactable = false;
            limitText.color = Color.red;
        }
        else
        {
            recruitButton.interactable = true;
            limitText.color = Color.black;
        }
    }

    private int CountCurrentSize(List<Unit> units)
    {
        int sum = 0;

        foreach (var unit in units)
        {
            sum += unit.SizeInArmy;
        }

        return sum;
    }

    private void OnEnable()
    {
        UnitAssignController.UnitAmountChanged += HandleUnitAmountChanged;
    }

    private void OnDisable()
    {
        UnitAssignController.UnitAmountChanged -= HandleUnitAmountChanged;
    }
}