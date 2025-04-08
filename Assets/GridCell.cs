using UnityEngine;

public class GridCell : MonoBehaviour
{
    private int posX;
    private int posY;

    private float cellSize;

    public GameObject objectInThisGridSpace = null;

    private bool isPossibleMove;


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
        objectInThisGridSpace = Instantiate(piece, new Vector4(pos.x, 0.2F, pos.z + cellSize), Quaternion.identity);
    }

    public void SetAndMovePiece(GameObject piece, Vector3 position)
    {
        objectInThisGridSpace = piece;
        objectInThisGridSpace.GetComponent<Piece>().SetTargetPosition(new Vector3(position.x, 0.2F, position.z + cellSize));
    }

    public Vector2Int GetPosition()
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