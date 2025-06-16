using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class TutorialGrid : MonoBehaviour
{
    [SerializeField] public int width, height;

    [SerializeField] private GameObject gridCell;

    [SerializeField] private Transform cameraPosition;

    [SerializeField] private BoardManager boardManager;

    [SerializeField] private FileManager fileManager;

    [SerializeField] private TextMeshProUGUI tutorialText;

    private readonly float gridCellSize = 2;

    private List<Position> tutorialPossibleMoves = new();

    private GameObject[,] tutorialGrid;

    private List<Tuple<string, string>> tutorialMessages;

    public void Start()
    {
        GenerateField();
        if (StaticData.tutorial)
        {
            cameraPosition.transform.SetPositionAndRotation(new Vector3(-2, 28, 15), Quaternion.Euler(90, 0, 0));
        }

    }

    public void GenerateField()
    {
        float campSpacing = 1.0F + gridCellSize * 3;
        tutorialGrid = new GameObject[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tutorialGrid[x, y] = Instantiate(gridCell, new Vector4(x * gridCellSize, 0, y * gridCellSize + campSpacing), Quaternion.identity);
                GridCell cell = tutorialGrid[x, y].GetComponent<GridCell>();
                cell.InitializeGridCell(x, y, gridCellSize);
                cell.SetPosition(x, y);
                tutorialGrid[x, y].transform.parent = transform;
                tutorialGrid[x, y].transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }

        this.transform.position = new Vector3(this.transform.position.x - 20, this.transform.position.y, this.transform.position.z - 5);
    }

    public void InitializePieces(string name, bool isDrop, int[] moveset)
    {
        if (tutorialMessages == null || tutorialMessages.Count == 0)
        {
            tutorialMessages = fileManager.GetTutorialMessages();
            tutorialText.text = tutorialMessages[0].Item2;
        }
        if (tutorialGrid[2, 2].GetComponent<GridCell>().objectInThisGridSpace != null)
        {
            Destroy(tutorialGrid[2, 2].GetComponent<GridCell>().objectInThisGridSpace);
        }

        var resource = Resources.Load("Prefabs/Piece/" + name + "Piece") as GameObject;
        if (resource != null)
        {
            tutorialPossibleMoves.Clear();
            var cell = tutorialGrid[2, 2].GetComponent<GridCell>();
            cell.SetPiece(resource);
            cell.objectInThisGridSpace.transform.rotation = Quaternion.Euler(0, 180, 0);
            var piece = cell.objectInThisGridSpace.GetComponent<Piece>();
            piece.InitializePiece(name, moveset, 2, 2, false);
            cell.objectInThisGridSpace.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            tutorialPossibleMoves = boardManager.CalculatePossibleMoves(piece, true);
        }
        if (isDrop)
        {
            tutorialText.text = tutorialMessages[1].Item2;
        }
    }

    public void SetPromotionTutorialMessage()
    {
        tutorialText.text = tutorialMessages[2].Item2;
    }

    public void SetAbilityUsageMessage(string pieceName)
    {
        switch (pieceName)
        {
            case "Rook":
                tutorialText.text = tutorialMessages[3].Item2;
                break;
            case "GoldGeneral":
                tutorialText.text = tutorialMessages[4].Item2;
                break;
            case "SilverGeneral":
                tutorialText.text = tutorialMessages[5].Item2;
                break;
            case "King":
                tutorialText.text = tutorialMessages[6].Item2;
                break;
            default:
                break;
        }
    }

    public void OnHoverExitRestoreDefaultColor()
    {
        Color defaultColor = new(0.04f, 0.43f, 0.96f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tutorialGrid[x, y].GetComponentInChildren<SpriteRenderer>().material.color = defaultColor;
            }
        }
    }

    public void ClearPossibleMoves()
    {
        OnHoverExitRestoreDefaultColor();
        if (tutorialPossibleMoves != null && tutorialPossibleMoves.Count != 0)
        {
            foreach (var item in tutorialPossibleMoves)
            {
                if (item.x >= 0 && item.x < 5 && item.y >= 0 && item.y < 5)
                {
                    tutorialGrid[item.x, item.y].GetComponentInChildren<SpriteRenderer>().material.color = Color.green;
                }
            }
        }
    }
}