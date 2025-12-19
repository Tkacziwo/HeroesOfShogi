using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleDeploymentController : MonoBehaviour
{
    [SerializeField] private UnitSlot slotPrefab;

    [SerializeField] private DragDropController unitDragPrefab;

    [SerializeField] private GameObject textPrefab;

    [SerializeField] private Canvas canvasRef;

    private PlayerCharacterController playerCharacter;

    private UnitSlot[,] unitSlots;

    private DragDropController[,] availableUnits;

    [SerializeField] private int tileSize = 80;

    private Dictionary<UnitEnum, int> unitDict;

    private List<Sprite> unitIcons;

    public static Action<bool> OnBattleStarted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.unitIcons = StaticData.unitIcons;

        playerCharacter = BattleDeploymentStaticData.playerCharacter;

        GenerateGridLayout();

        if (playerCharacter == null) return;
        GenerateUnitLayout();
    }

    private void GenerateGridLayout()
    {
        unitSlots = new UnitSlot[3, 9];

        int startX = -tileSize - 5;

        int startY = (9 * (tileSize + 5)) / 2 - 100;

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                unitSlots[y, x] = Instantiate(slotPrefab);
                unitSlots[y, x].transform.SetParent(this.transform);
                unitSlots[y, x].GetComponent<RectTransform>().anchoredPosition = new(startX, startY);
                startY -= tileSize + 5;
            }

            startX += tileSize + 5;
            startY = 9 * (tileSize + 5) / 2 - 100;
        }
    }

    private void GenerateUnitLayout()
    {
        var units = playerCharacter.GetAssignedUnits();

        var readyDict = SplitUnitsToDict(units);

        if (readyDict == null) return;

        var maxVal = 0;



        foreach (var dict in readyDict)
        {
            if (maxVal < dict.Value.Count)
            {
                maxVal = dict.Value.Count;
            }
        }


        int startY = tileSize * 3 + 50;

        foreach (var dict in readyDict)
        {
            int startX = -900;
            var text = Instantiate(textPrefab);
            text.GetComponent<TextMeshProUGUI>().text = dict.Key.ToString() + ":";
            text.transform.SetParent(canvasRef.transform);
            text.GetComponent<RectTransform>().anchoredPosition = new(-830, startY + 60);

            for (int x = 0; x < dict.Value.Count; x++)
            {
                var unit = Instantiate(unitDragPrefab);
                var script = unit.GetComponent<DragDropController>();
                script.canvasRef = canvasRef;
                script.assignedUnit = dict.Value[x];
                unit.transform.SetParent(canvasRef.transform);
                unit.GetComponent<RectTransform>().anchoredPosition = new(startX, startY);
                script.image.sprite = unitIcons.SingleOrDefault(o => o.name == script.assignedUnit.UnitName.ToString());

                startX += tileSize + 5;
            }

            startY -= tileSize + 65;
            startX = -900;
        }
    }

    private Dictionary<UnitEnum, List<Unit>> SplitUnitsToDict(List<Unit> units)
    {
        Dictionary<UnitEnum, List<Unit>> res = new();

        foreach (var unit in units)
        {
            if (res.ContainsKey(unit.UnitName))
            {
                res[unit.UnitName].Add(unit);
            }
            else
            {
                res.Add(unit.UnitName, new() { unit });
            }
        }

        return res;
    }

    public void OnBattleStart()
    {
        Unit[,] units = new Unit[3, 9];

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                var slot = unitSlots[y, x];
                if (slot.droppedUnit != null)
                {
                    units[y, x] = slot.droppedUnit.assignedUnit;
                }
            }
        }

        BattleDeploymentStaticData.playerFormation = units;


        OnBattleStarted?.Invoke(false);

        SceneManager.UnloadSceneAsync("BattleDeployment");

        SceneManager.LoadScene("Game", LoadSceneMode.Additive);
    }
}
