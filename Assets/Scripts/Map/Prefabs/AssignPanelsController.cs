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

    public static Action<Tuple<string, ProducedUnits>> OnUnitsRecruit;

    [SerializeField] private GameObject pawnPanel;

    [SerializeField] private GameObject lancePanel;

    [SerializeField] private GameObject horsePanel;

    [SerializeField] private GameObject silverGeneralPanel;

    [SerializeField] private GameObject goldGeneralPanel;

    [SerializeField] private GameObject rookPanel;

    [SerializeField] private GameObject bishopPanel;

    [SerializeField] private Sprite tempSprite;

    private readonly ProducedUnits selectedUnits = new();

    private ProducedUnits cityUnits = new();

    private List<Sprite> unitIcons = new();

    private readonly List<Unit> unitTemplates = StaticData.unitTemplates;

    private string cityName;

    public void Setup(PlayerCharacterController currentCharacter, ProducedUnits cityUnits, string cityName)
    {
        this.cityName = cityName;
        unitIcons = StaticData.unitIcons;
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
        pawnPanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.pawns, GetSpriteForUnit(UnitEnum.Pawn), unitTemplates.Single(o => o.UnitName == UnitEnum.Pawn));

        lancePanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.lances, GetSpriteForUnit(UnitEnum.Lance), unitTemplates.Single(o => o.UnitName == UnitEnum.Lance));

        horsePanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.horses, GetSpriteForUnit(UnitEnum.Horse), unitTemplates.Single(o => o.UnitName == UnitEnum.Horse));

        silverGeneralPanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.silverGenerals, GetSpriteForUnit(UnitEnum.SilverGeneral), unitTemplates.Single(o => o.UnitName == UnitEnum.SilverGeneral));

        goldGeneralPanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.goldGenerals, GetSpriteForUnit(UnitEnum.GoldGeneral), unitTemplates.Single(o => o.UnitName == UnitEnum.GoldGeneral));

        rookPanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.rooks, GetSpriteForUnit(UnitEnum.Rook), unitTemplates.Single(o => o.UnitName == UnitEnum.Rook));

        bishopPanel.GetComponentInChildren<UnitAssignController>().Setup(cityUnits.bishops, GetSpriteForUnit(UnitEnum.Bishop), unitTemplates.Single(o => o.UnitName == UnitEnum.Bishop));
    }

    private Sprite GetSpriteForUnit(UnitEnum unitEnum)
    {
        Sprite chosenSprite = unitIcons.SingleOrDefault(o => o.name == unitEnum.ToString());
        if(chosenSprite == null)
        {
            return tempSprite;
        }
        else
        {
            return chosenSprite;
        }
    }

    public void OnClick()
    {
        if (!recruitButton.interactable) return;

        currentCharacter.SetUnits(tempUnits);

        cityUnits.pawns -= selectedUnits.pawns;
        cityUnits.lances -= selectedUnits.lances;
        cityUnits.horses -= selectedUnits.horses;
        cityUnits.silverGenerals -= selectedUnits.silverGenerals;
        cityUnits.goldGenerals -= selectedUnits.goldGenerals;
        cityUnits.rooks -= selectedUnits.rooks;
        cityUnits.bishops -= selectedUnits.bishops;

        OnUnitsRecruit?.Invoke(new(cityName, cityUnits));

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
        else if (unit.UnitName == UnitEnum.SilverGeneral)
        {
            selectedUnits.silverGenerals = amount;
        }
        else if (unit.UnitName == UnitEnum.GoldGeneral)
        {
            selectedUnits.goldGenerals = amount;
        }
        else if (unit.UnitName == UnitEnum.Rook)
        {
            selectedUnits.rooks = amount;
        }
        else if (unit.UnitName == UnitEnum.Bishop)
        {
            selectedUnits.bishops = amount;
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
        => units.Select(o => o.SizeInArmy).Sum();
    

    private void OnEnable()
    {
        UnitAssignController.UnitAmountChanged += HandleUnitAmountChanged;
    }

    private void OnDisable()
    {
        UnitAssignController.UnitAmountChanged -= HandleUnitAmountChanged;
    }
}