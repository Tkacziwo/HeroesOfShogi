using System;
using System.Collections.Generic;
using UnityEngine;

public class GridGame : MonoBehaviour
{
    [SerializeField] public int width, height;

    [SerializeField] private GameObject gridCell;

    [SerializeField] private Transform cameraPosition;

    [SerializeField] private float gridCellSize;

    [SerializeField] private GameObject piecePrefab;

    public float xRot = 40;

    public float yRot = -90;

    public GameObject[,] gameGrid;

    public GameObject[,] playerCamp;

    public int playerCampPos = 0;

    public int playerCampPosY;

    public int enemyCampPos = 0;

    public int enemyCampPosY;

    public GameObject[,] enemyCamp;

    private FileManager fileManager;

    public void Start()
    {
        gridCellSize *= 2;
        fileManager = FindFirstObjectByType<FileManager>();
        GenerateField();
        InitializePieces();
        playerCampPosY = 2;
        enemyCampPosY = 0;
    }

    public void GenerateField()
    {
        playerCamp = new GameObject[9, 3];
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                playerCamp[x, y] = Instantiate(gridCell, new Vector4(x * gridCellSize, 0, y * gridCellSize), Quaternion.identity);
                GridCell cell = playerCamp[x, y].GetComponent<GridCell>();
                cell.InitializeGridCell(x, y, gridCellSize);
                cell.SetPosition(x, y);
                playerCamp[x, y].transform.parent = transform;
                playerCamp[x, y].transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
        float campSpacing = 5.0F + gridCellSize * 3;
        gameGrid = new GameObject[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                gameGrid[x, y] = Instantiate(gridCell, new Vector4(x * gridCellSize, 0, y * gridCellSize + campSpacing), Quaternion.identity);
                GridCell cell = gameGrid[x, y].GetComponent<GridCell>();
                cell.InitializeGridCell(x, y, gridCellSize);
                cell.SetPosition(x, y);
                gameGrid[x, y].transform.parent = transform;
                gameGrid[x, y].transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
        campSpacing += 9 * gridCellSize + 5.0F;
        enemyCamp = new GameObject[9, 3];
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                enemyCamp[x, y] = Instantiate(gridCell, new Vector4(x * gridCellSize, 0, y * gridCellSize + campSpacing), Quaternion.identity);
                GridCell cell = enemyCamp[x, y].GetComponent<GridCell>();
                cell.InitializeGridCell(x, y, gridCellSize);
                cell.SetPosition(x, y);
                enemyCamp[x, y].transform.parent = transform;
                enemyCamp[x, y].transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }


        float xRot = 63.0F;
        float yRot = -90;

        cameraPosition.position = new Vector4((float)100, 80, ((height + 6) * gridCellSize + 10.0F) / 2 - gridCellSize / 2);
        cameraPosition.rotation = Quaternion.Euler(xRot, yRot, 0);
    }

    public void InitializePieces()
    {
        var piecesPositions = fileManager.PiecesPositions.boardPositions;

        foreach (var p in piecesPositions)
        {
            var cell = gameGrid[p.posX, p.posY].GetComponent<GridCell>();
            var resource = Resources.Load("ShogiPiece") as GameObject;
            var moveset = fileManager.GetMovesetByPieceName(p.piece);
            bool isSpecialPiece = SpecialPieceCheck(p.piece);
            cell.SetPiece(resource);
            var pieceScript = cell.objectInThisGridSpace.GetComponent<Piece>();
            pieceScript.InitializePiece(p.piece, moveset, cell.GetPosition().x, cell.GetPosition().y, isSpecialPiece);

            if (pieceScript.GetIsBlack())
            {
                cell.objectInThisGridSpace.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
            }
            else
            {
                cell.objectInThisGridSpace.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
        }
    }

    private bool SpecialPieceCheck(string name)
    {
        if (name == "Rook" || name == "Bishop")
        {
            return true;
        }
        return false;
    }

    public void AddToCamp(GameObject piece)
    {
        var pieceScript = piece.GetComponent<Piece>();
        
        if (pieceScript.GetIsPromoted())
        {
            pieceScript.Demote();
        }

        pieceScript.SetIsDrop();
        pieceScript.ReverseOriginalMovementMatrix();
        pieceScript.ReverseMovementMatrix();

        if (piece.GetComponent<Piece>().GetIsBlack())
        {
            piece.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            var cell = playerCamp[playerCampPos, playerCampPosY].GetComponent<GridCell>();
            cell.SetAndMovePiece(piece, cell.GetWorldPosition());
            playerCampPos++;
            if (playerCampPos == 9)
            {
                playerCampPos = 0;
                playerCampPosY--;
            }
        }
        else
        {
            piece.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
            var cell = enemyCamp[enemyCampPos, enemyCampPosY].GetComponent<GridCell>();
            cell.SetAndMovePiece(piece, cell.GetWorldPosition());
            enemyCampPos++;
            if (enemyCampPos == 9)
            {
                enemyCampPos = 0;
                enemyCampPosY++;
            }
        }
    }

    public void OnHoverExitRestoreDefaultColor()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                gameGrid[x, y].GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
            }
        }
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < width; x++)
            {
                playerCamp[x, y].GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
                enemyCamp[x, y].GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
            }
        }
    }

    public void ClearPossibleMoves(IList<Tuple<int, int>> blacklist = null)
    {
        if (blacklist == null)
        {
            OnHoverExitRestoreDefaultColor();
        }
        else
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    gameGrid[x, y].GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
                }
            }
            foreach (var item in blacklist)
            {
                gameGrid[item.Item1, item.Item2].GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
            }
        }
    }
}
