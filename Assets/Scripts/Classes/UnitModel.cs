using System.Collections.Generic;
using UnityEngine;

public class UnitModel : MonoBehaviour
{
    public Unit Unit { get; set; } = new();

    public GameObject Model;

    private List<Vector3> curvePath = new();

    private int pathIterator = 0;

    private float movementSpeed = 20f;

    public void InitUnit(string name, int[] moveset, int x, int y, bool isSpecial, float movementSpeed)
    {
        Unit.InitPiece(name, moveset, x, y, isSpecial);
        this.movementSpeed = movementSpeed;
    }

    public void SetPath(List<Vector3> path)
    {
        this.curvePath = path;
        pathIterator = 0;
    }

    private void Update()
    {
        if (curvePath.Count == 0) return;

        if (Vector3.Distance(Model.transform.position, curvePath[pathIterator]) >= 0000.1f)
        {
            float step = Time.deltaTime * movementSpeed;
            MakeStep(curvePath[pathIterator], step);
        }
        else
        {
            pathIterator++;

            if (pathIterator >= curvePath.Count) { curvePath.Clear(); }
        }
    }

    private void MakeStep(Vector3 targetPosition, float step)
        => Model.transform.position = Vector3.MoveTowards(Model.transform.position, targetPosition, step);
}