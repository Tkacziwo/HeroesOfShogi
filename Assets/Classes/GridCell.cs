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

    /// <summary>
    /// Using quadratic transformation change position of piece to destination.
    /// </summary>
    /// <param name="piece">piece to move</param>
    /// <param name="position">destination position</param>
    public void SetAndMovePiece(GameObject piece, Vector3 position)
    {
        objectInThisGridSpace = piece;
        objectInThisGridSpace.GetComponent<Piece>().QuadraticTransformation(new Vector3(position.x, 0.2F, position.z));
    }

    /// <summary>
    /// Using linear transformation change position of piece to destination.
    /// </summary>
    /// <param name="piece">piece to move</param>
    /// <param name="position">destination position</param>
    public void SetAndMovePieceLinear(GameObject piece, Vector3 position)
    {
        objectInThisGridSpace = piece;
        objectInThisGridSpace.GetComponent<Piece>().LinearTransformation(new Vector3(position.x, 0.2F, position.z));
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