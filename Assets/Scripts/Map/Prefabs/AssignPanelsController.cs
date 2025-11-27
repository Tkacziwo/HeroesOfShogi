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


    private readonly List<Unit> unitTemplates = StaticData.unitTemplates;

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
        pawnPanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.pawns, tempSprite, unitTemplates.Single(o => o.UnitName == UnitEnum.Pawn));

        lancePanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.lances, tempSprite, unitTemplates.Single(o => o.UnitName == UnitEnum.Lance));

        horsePanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.horses, tempSprite, unitTemplates.Single(o => o.UnitName == UnitEnum.Horse));

        silverGeneralPanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.silverGenerals, tempSprite, unitTemplates.Single(o => o.UnitName == UnitEnum.SilverGeneral));

        goldGeneralPanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.goldGenerals, tempSprite, unitTemplates.Single(o => o.UnitName == UnitEnum.GoldGeneral));

        rookPanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.rooks, tempSprite, unitTemplates.Single(o => o.UnitName == UnitEnum.Rook));

        bishopPanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.bishops, tempSprite, unitTemplates.Single(o => o.UnitName == UnitEnum.Bishop));
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
            tempUnits.Add(unitTemplates.Single(o => o.UnitName == UnitEnum.Pawn));
        }
        for (int i = 0; i < selectedUnits.lances; i++)
        {
            tempUnits.Add(unitTemplates.Single(o => o.UnitName == UnitEnum.Lance));
        }
        for (int i = 0; i < selectedUnits.horses; i++)
        {
            tempUnits.Add(unitTemplates.Single(o => o.UnitName == UnitEnum.Horse));
        }
        for (int i = 0; i < selectedUnits.goldGenerals; i++)
        {
            tempUnits.Add(unitTemplates.Single(o => o.UnitName == UnitEnum.GoldGeneral));
        }
        for (int i = 0; i < selectedUnits.silverGenerals; i++)
        {
            tempUnits.Add(unitTemplates.Single(o => o.UnitName == UnitEnum.SilverGeneral));
        }
        for (int i = 0; i < selectedUnits.rooks; i++)
        {
            tempUnits.Add(unitTemplates.Single(o => o.UnitName == UnitEnum.Rook));
        }
        for (int i = 0; i < selectedUnits.bishops; i++)
        {
            tempUnits.Add(unitTemplates.Single(o => o.UnitName == UnitEnum.Bishop));
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