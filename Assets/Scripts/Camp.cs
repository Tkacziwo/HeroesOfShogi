using System.Collections.Generic;
using UnityEngine;

public class Camp : MonoBehaviour
{
    public GameObject[,] campGrid;

    [SerializeField] private GameObject gridCell = null;

    [SerializeField] private float gridCellSize;

    public int numberOfPieces = 0;

    public int posX = 0;

    private int posY;

    private int originalPosY;

    public int positionOperator;

    public List<GameObject> capturedPieceObjects;

    void Start()
    {
        capturedPieceObjects = new();
        posX = numberOfPieces = 0;
    }

    public void InitializePosY(int posY)
    {
        this.posY = posY;
        originalPosY = posY;
    }

    public void InitializeGrid(int posY, float gridCellSize, GameObject gridCell)
    {
        this.gridCellSize = gridCellSize;
        this.gridCell = gridCell;
        this.posY = posY;
        if (this.posY == 2)
        {
            positionOperator = -1;
        }
        else
        {
            positionOperator = 1;
        }
    }

    public void GenerateCamp(float spacing = 0)
    {
        campGrid = new GameObject[9, 3];
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                campGrid[x, y] = Instantiate(gridCell, new Vector4(x * gridCellSize, 0, y * gridCellSize + spacing), Quaternion.identity);
                GridCell cell = campGrid[x, y].GetComponent<GridCell>();
                cell.InitializeGridCell(x, y, gridCellSize);
                cell.SetPosition(x, y);
                campGrid[x, y].transform.parent = transform;
                campGrid[x, y].transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
    }

    public void Reshuffle()
    {
        posX = 0;
        posY = originalPosY;
        numberOfPieces = 0;

        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                campGrid[x, y].GetComponent<GridCell>().objectInThisGridSpace = null;
            }
        }

        foreach (var pieceObject in capturedPieceObjects)
        {
            var pieceScript = pieceObject.GetComponent<Piece>();
            pieceScript.SetIsDrop();

            var cell = campGrid[posX, posY].GetComponent<GridCell>();
            cell.SetAndMovePieceLinear(pieceObject, cell.GetWorldPosition());
            posX++;
            numberOfPieces++;
            if (posX == 9)
            {
                posX = 0;
                posY += positionOperator;
            }
        }
    }

    public void AddToCamp(GameObject piece)
    {
        var pieceScript = piece.GetComponent<Piece>();

        if (pieceScript.GetIsPromoted())
        {
            pieceScript.Demote();
        }

        pieceScript.SetIsDrop();
        pieceScript.ReverseMovementMatrix();

        if (pieceScript.GetIsBlack())
        {
            pieceScript.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            pieceScript.ResetIsBlack();
        }
        else
        {
            pieceScript.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
            pieceScript.SetIsBlack();
        }
        var cell = campGrid[posX, posY].GetComponent<GridCell>();
        var aboveCellPosition = cell.GetWorldPosition();
        aboveCellPosition.y += 10;
        piece.GetComponent<Piece>().SetPiecePositionImmediate(aboveCellPosition);

        cell.SetAndMovePieceLinear(piece, cell.GetWorldPosition());
        posX++;
        numberOfPieces++;
        if (posX == 9)
        {
            posX = 0;
            posY += positionOperator;
        }
        capturedPieceObjects.Add(piece);
    }
}
