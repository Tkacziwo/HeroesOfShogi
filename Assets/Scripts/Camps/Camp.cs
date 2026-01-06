using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Camp which holds killed pieces for dropping back onto the board.
/// </summary>
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

    public List<UnitModel> capturedPieceObjects;

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

    public void GenerateCamp(float spacing = 0)
    {
        campGrid = new GameObject[9, 3];
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                campGrid[x, y] = Instantiate(gridCell, new Vector4(x * gridCellSize, 11.2f, y * gridCellSize + spacing), Quaternion.identity);
                GridCell cell = campGrid[x, y].GetComponent<GridCell>();
                cell.InitializeGridCell(x, y);
                cell.SetPosition(x, y);
                campGrid[x, y].transform.parent = transform;
                campGrid[x, y].transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
    }

    /// <summary>
    /// Reshuffles the camp to remove any empty cells between captured units.
    /// </summary>
    public void Reshuffle()
    {
        posX = 0;
        posY = originalPosY;
        numberOfPieces = 0;

        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                campGrid[x, y].GetComponent<GridCell>().unitInCell = null;
            }
        }

        foreach (var pieceObject in capturedPieceObjects)
        {
            var pieceScript = pieceObject.Unit;
            pieceScript.SetIsDrop();

            var cell = campGrid[posX, posY].GetComponent<GridCell>();
            cell.unitInCell = pieceObject;
            var cellWorldPosition = cell.GetWorldPosition();
            cell.unitInCell.transform.position = new(cellWorldPosition.x, 11.2f, cellWorldPosition.z);
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

    /// <summary>
    /// Adds killed unit to enemy camp.
    /// </summary>
    /// <param name="unitModel">Killed unit</param>
    public void AddToCamp(UnitModel unitModel)
    {
        unitModel.promotionEffect.GetComponent<ParticleSystem>().Stop();
        if (capturedPieceObjects.Count == 27)
        {
            Destroy(unitModel);
        }

        var pieceScript = unitModel.Unit;

        if (pieceScript.GetIsPromoted())
        {
            pieceScript.Demote();
        }

        pieceScript.SetIsDrop();
        pieceScript.ReverseMovementMatrix();

        if (pieceScript.GetIsBlack())
        {
            unitModel.pieceIcon.GetComponent<MeshRenderer>().material.color = Color.white;
            pieceScript.ResetIsBlack();
        }
        else
        {
            unitModel.pieceIcon.GetComponent<MeshRenderer>().material.color = Color.black;
            pieceScript.SetIsBlack();
        }
        var cell = campGrid[posX, posY].GetComponent<GridCell>();
        var aboveCellPosition = cell.GetWorldPosition();
        //aboveCellPosition.y += 10;
        unitModel.Model.transform.position = aboveCellPosition;
        if(pieceScript.GetIsBlack())
        {
            unitModel.Model.transform.SetPositionAndRotation(aboveCellPosition, Quaternion.Euler(-90,0,0));
        }
        else
        {
            //unitModel.Model.transform.rotation = Quaternion.Euler(-90,-180,0);
            unitModel.Model.transform.SetPositionAndRotation(aboveCellPosition, Quaternion.Euler(-90, -180, 0));
        }
        unitModel.UpdateHealthBarPosition();
        unitModel.UpdateParticleSystemPosition();
        cell.unitInCell = unitModel;

        unitModel.Unit.RestoreMaxHP();
        unitModel.healthBar.GetComponent<HealthBarController>().UpdateHealthBar(unitModel.Unit.HealthPoints);
        //piece.SetPiecePositionImmediate(aboveCellPosition);

        //cell.SetAndMovePieceLinear(piece, cell.GetWorldPosition());
        posX++;
        numberOfPieces++;
        if (posX == 9)
        {
            posX = 0;
            posY += positionOperator;
        }
        capturedPieceObjects.Add(unitModel);
    }
}
