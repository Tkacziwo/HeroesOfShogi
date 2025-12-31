using System;
using UnityEngine;

/// <summary>
/// Represents single grid cell on the board
/// </summary>
public class GridCell : MonoBehaviour
{
    private int posX;
    private int posY;

    private float cellSize;

    public GameObject objectInThisGridSpace = null;

    public UnitModel unitInGridCell;

    private bool isPossibleMove;

    /// <summary>
    /// Initializes grid cell with values.
    /// </summary>
    /// <param name="x">x position</param>
    /// <param name="y">y position</param>
    /// <param name="cellSize">size of the cell</param>
    public void InitializeGridCell(int x, int y, float cellSize)
    {
        posX = x;
        posY = y;
        this.cellSize = cellSize;
        isPossibleMove = false;
    }

    public void SetPosition(int x, int y)
    {
        posX = x;
        posY = y;
    }

    public void SetPiece(GameObject piece)
    {
        var pos = GetWorldPosition();
        objectInThisGridSpace = Instantiate(piece, new Vector4(pos.x, 0.2F, pos.z), Quaternion.identity);
    }

    public void SetUnit(UnitModel unit)
    {
        var pos = GetWorldPosition();
        unitInGridCell = Instantiate(unit, new Vector4(pos.x, 11.2f, pos.z), Quaternion.identity);
    }

    /// <summary>
    /// Calculates path from Model position to target position using Bezier curves and applies it to unit.
    /// </summary>
    /// <param name="unit">Unit to move</param>
    /// <param name="position">Destination position</param>
    public void SetAndMovePiece(UnitModel unit, Vector3 position)
    {
        unitInGridCell = unit;
        var path = TransformationCalculator.QuadraticTransformation(unitInGridCell.Model.transform.position, new Vector3(position.x, 11.2f, position.z));

        unitInGridCell.SetPath(path);
        
    }

    /// <summary>
    /// Using linear transformation change position of piece to destination.
    /// </summary>
    /// <param name="piece">piece to move</param>
    /// <param name="position">destination position</param>
    public void SetAndMovePieceLinear(UnitModel piece, Vector3 position)
    {
        unitInGridCell = piece;
        unitInGridCell.transform.position = new(position.x, 11.2f, position.z);
        //unitInGridCell.GetComponent<Unit>().LinearTransformation(new Vector3(position.x, 11.2f, position.z));
    }

    public Position GetPosition()
        => new(posX, posY);

    public Vector3 GetWorldPosition()
        => transform.position;

    public bool GetIsPossibleMove()
        => isPossibleMove;

    public bool SetIsPossibleMove()
        => isPossibleMove = true;

    public bool ResetIsPossibleMove()
        => isPossibleMove = false;
}