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

    private FileManager fileManager;

    public void Start()
    {
        gridCellSize *= 2;
        fileManager = FindFirstObjectByType<FileManager>();
        GenerateField();
        InitializePieces();
    }

    public void GenerateField()
    {
        gameGrid = new GameObject[width, height];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                gameGrid[j, i] = Instantiate(gridCell, new Vector4(j * gridCellSize, 0, i * gridCellSize), Quaternion.identity);
                GridCell cell = gameGrid[j, i].GetComponent<GridCell>();
                cell.InitializeGridCell(j, i, gridCellSize);
                cell.SetPosition(j, i);
                gameGrid[j, i].transform.parent = transform;
                gameGrid[j, i].transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }

        float xRot = 55;
        float yRot = -90;

        cameraPosition.position = new Vector4((float)100, 65, (float)height * gridCellSize / 2 - gridCellSize / 2);
        cameraPosition.rotation = Quaternion.Euler(xRot, yRot, 0);
    }

    public void InitializePieces()
    {
        var piecesPositions = fileManager.PiecesPositions.boardPositions;

        foreach (var p in piecesPositions)
        {
            var cell = gameGrid[p.posX, p.posY].GetComponent<GridCell>();
            var resource = Resources.Load("ShogiPiece") as GameObject;
            cell.SetPiece(resource);
            var pieceScript = cell.objectInThisGridSpace.GetComponent<Piece>();
            var moveset = fileManager.GetMovesetByPieceName(p.piece);
            pieceScript.InitializePiece(p.piece, moveset, cell.GetPosition().x, cell.GetPosition().y);
        }
        //for (int x = 0; x < 9; x++)
        //{
        //    var cell = gameGrid[x, 2].GetComponent<GridCell>();
        //    var resource = Resources.Load("ShogiPiece") as GameObject;
        //    cell.SetPiece(resource);
        //    var pieceScript = cell.objectInThisGridSpace.GetComponent<Piece>();
        //    var moveset = fileManager.GetMovesetByPieceName("Pawn");
        //    pieceScript.InitializePiece("Pawn", moveset, cell.GetPosition().x, cell.GetPosition().y);
        //}
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
