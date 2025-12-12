using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerCharacterController : MonoBehaviour
{
    public int characterId;

    [SerializeField]
    private string Name;

    public int movementPoints;

    public int usedMovementPointsForCurrentTurn = 0;

    public Vector3Int characterPosition;

    public int armySizeLimit = 30;

    public List<Unit> AssignedUnits { get; set; } = new();

    private Vector3 targetPosition;

    [SerializeField]
    private float movementSpeed;

    private bool isMoving;

    private List<Vector3> path = new();

    private int pathIterator;

    public int playerId;

    public Color playerColor;

    public static event Action<Vector3> OnPlayerOverTile;

    public event Action<Transform> OnPlayerMoveUpdateCameraPosition;

    public Vector3Int unreachedBotDestination;

    public InteractibleBuilding unreachedBuilding;
    public void SetPlayerTransform(Vector3 newTransform)
        => this.transform.position = newTransform;

    public void OnEnable()
    {
        OverworldMapController.onTurnEnd += HandleEndTurn;
    }

    public void OnDisable()
    {
        OverworldMapController.onTurnEnd -= HandleEndTurn;
    }


    public void SetPlayerPosition(Vector3Int newPos)
    {
        characterPosition = newPos;
    }

    public void HandleEndTurn()
    {
        ClearPath();
        ResetUsedMovementPoints();
        targetPosition = transform.position;
    }

    public void SetPath(List<Vector3> path)
    {
        if (path.Count == 0) return;
        pathIterator = 0;
        this.path = new(path);
        isMoving = true;
        SetTargetPosition(path[0]);
    }

    public void SetTargetPosition(Vector3 p)
    {
        targetPosition = p;
    }

    public Vector3 GetTargetPosition()
        => targetPosition;

    public void MakeStep(float step)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        OnPlayerMoveUpdateCameraPosition?.Invoke(transform);
    }

    public void SetIsMoving(bool isMoving)
        => this.isMoving = isMoving;

    public int GetMovementPoints()
        => this.movementPoints;

    public int GetUsedMovementPointsForCurrentTurn()
        => this.usedMovementPointsForCurrentTurn;

    public void ResetUsedMovementPoints()
        => this.usedMovementPointsForCurrentTurn = 0;

    public int GetRemainingMovementPoints()
        => movementPoints - usedMovementPointsForCurrentTurn;

    public int GetPathIterator()
        => pathIterator;

    public void ClearPath()
        => path.Clear();

    public void ReduceAvailableMovementPoints(int amount)
        => usedMovementPointsForCurrentTurn += Math.Abs(amount);

    public List<Unit> GetAssignedUnits()
        => AssignedUnits;

    public void SetUnits(List<Unit> units)
        => AssignedUnits = units;

    void Update()
    {
        if (isMoving)
        {
            if (Vector3.Distance(transform.position, targetPosition) >= 0.001f)
            {
                var step = Time.deltaTime * movementSpeed;
                MakeStep(step);
            }
            else
            {
                pathIterator++;

                if (pathIterator < path.Count)
                {
                    OnPlayerOverTile?.Invoke(path[pathIterator - 1]);
                    SetPlayerTransform(targetPosition);
                    SetTargetPosition(path[pathIterator]);
                }
                else
                {
                    isMoving = false;
                    PlayerEvents.OnPlayerEndMove?.Invoke(this);
                    path.Clear();
                }
            }
        }
    }
}
