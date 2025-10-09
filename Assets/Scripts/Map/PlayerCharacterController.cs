using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField]
    private string Name;

    [SerializeField]
    private double MovementPoints;

    public Vector3Int characterPosition;

    private List<Unit> AssignedUnits { get; set; } = new();

    private Vector3 targetPosition;

    [SerializeField]
    private float movementSpeed;

    private bool isMoving;

    private List<Vector3> path;

    private List<Vector3Int> tilesPositions;

    private int pathIterator;

    public int playerId;

    public Color playerColor;

    public static event Action<Vector3Int> OnPlayerOverTile;

    public void SetPlayerPosition(Vector3 newPos)
    {
        var p = newPos;
        //p.x += 0.5f;
        //p.y = 0.1f;
        //p.z += 0.5f;
        this.transform.position = p;
    }

    public void SetPath(List<Vector3> path, List<Vector3Int> tiles)
    {
        if (path.Count == 0)
        {
            return;
        }

        pathIterator = 0;
        this.tilesPositions = tiles;
        this.path = new(path);
        isMoving = true;
        SetTargetPosition(path[0]);
    }

    public void SetTargetPosition(Vector3 p)
    {
        targetPosition = p;
        //targetPosition.x += 0.5f;
        //targetPosition.y = 0.1f;
        //targetPosition.z += 0.5f;
    }

    public void MakeStep(float step)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
    }

    public void SetIsMoving(bool isMoving)
        => this.isMoving = isMoving;


    // Update is called once per frame
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
                    OnPlayerOverTile?.Invoke(tilesPositions[pathIterator - 1]);
                    SetPlayerPosition(targetPosition);
                    SetTargetPosition(path[pathIterator]);
                }
                else
                {
                    isMoving = false;
                    path.Clear();
                    PlayerEvents.OnPlayerEndMove?.Invoke(this);
                }
            }
        }
    }
}
