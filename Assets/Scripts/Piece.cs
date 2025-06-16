using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private string pieceName;

    private string originalPieceName;

    private int[] Moveset;

    private int[] originalMoveset;

    private int posX;

    private int posY;

    private bool isSpecial;

    private bool isDrop;

    private bool isBodyguard;

    public bool isKing;

    private GameObject emptyGameObject;

    [SerializeField] private float speed;

    private bool isBlack;

    private bool isPromoted;

    public int value;

    public int abilityCooldown;

    public GameObject promotionEffect;

    private readonly List<Vector3> curvePath = new();

    public bool finishedMoving = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        emptyGameObject = new()
        {
            name = posX + " " + posY + "pieceTargetDestination"
        };
        emptyGameObject.transform.position = transform.position;
    }

    public void InitializePiece(string name, int[] moveset, int x, int y, bool isSpecialPiece)
    {
        switch (name)
        {
            case "Pawn":
                value = 100;
                break;
            case "GoldGeneral":
                value = 500;
                break;
            case "SilverGeneral":
                value = 400;
                break;
            case "Bishop":
                value = 800;
                break;
            case "Rook":
                value = 800;
                break;
            case "Lance":
                value = 400;
                break;
            case "Horse":
                value = 300;
                break;
            case "King":
                value = 790;
                break;
            default:
                value = 0;
                break;
        }

        promotionEffect = Resources.Load("Prefabs/ParticleEffects/PromotionEffect") as GameObject;
        promotionEffect = Instantiate(promotionEffect);
        promotionEffect.GetComponent<ParticleSystem>().Stop();
        promotionEffect.GetComponent<ParticleSystem>().Clear();
        if (name == "King")
        {
            isKing = true;
        }
        else
        {
            isKing = false;
        }
        pieceName = name;
        originalPieceName = name;
        Moveset = moveset;
        BackupOriginalMoveset(moveset);
        posX = x;
        posY = y;
        isSpecial = isSpecialPiece;
        isPromoted = false;
        isDrop = false;
        isBodyguard = false;
        if (posY < 3)
        {
            isBlack = false;
        }
        else
        {
            ReverseMovementMatrix();
            isBlack = true;
        }
    }

    public void ReverseMovementMatrix()
    {
        for (int i = 0; i < 3; i++)
        {
            int temp = Moveset[i];
            Moveset[i] = Moveset[i + 6];
            Moveset[i + 6] = temp;
        }
    }

    public void ReverseOriginalMovementMatrix()
    {
        for (int i = 0; i < 3; i++)
        {
            int temp = originalMoveset[i];
            originalMoveset[i] = originalMoveset[i + 6];
            originalMoveset[i + 6] = temp;
        }
    }

    public void MovePiece(Position p)
    {
        posX = p.x;
        posY = p.y;
    }

    public int[] GetMoveset()
        => Moveset;

    public int[] GetOriginalMovest()
        => originalMoveset;

    public string GetName()
        => pieceName;

    public Position GetPosition()
        => new(posX, posY);

    public bool GetIsBlack()
        => isBlack;

    public void SetIsBlack()
        => isBlack = true;

    public void ResetIsBlack()
        => isBlack = false;

    public bool GetIsSpecial()
        => isSpecial;

    public bool GetIsPromoted()
        => isPromoted;

    public void SetPiecePositionImmediate(Vector3 target)
    {
        transform.position = target;
        emptyGameObject.transform.position = target;
    }

    public void SetTargetPosition(Vector3 target)
        => emptyGameObject.transform.position = target;

    public void Promote(int[] newMoveset)
    {
        promotionEffect.transform.position = this.transform.position;
        promotionEffect.GetComponent<ParticleSystem>().Play();
        Moveset = newMoveset;
        if (isBlack)
        {
            ReverseMovementMatrix();
        }
        isPromoted = true;
        //todo change textures
    }

    public void BackupOriginalMoveset(int[] moveset)
    {
        originalMoveset = new int[moveset.Length];
        for (int i = 0; i < moveset.Length; i++)
        {
            originalMoveset[i] = moveset[i];
        }
    }

    public void Demote()
    {
        promotionEffect.GetComponent<ParticleSystem>().Stop();
        pieceName = new(originalPieceName);
        Moveset = originalMoveset;
        isPromoted = false;
    }

    public bool GetIsDrop()
        => isDrop;

    public void SetIsDrop()
    {
        isDrop = true;
    }

    public bool GetIsBodyguard()
        => isBodyguard;

    public bool SetIsBodyguard()
        => isBodyguard = true;

    public bool ResetIsBodyguard()
        => isBodyguard = false;

    public void ResetIsDrop()
        => isDrop = false;

    public void LinearTransformation(Vector3 endPosition)
    {
        Vector3 startPosition = this.transform.position;
        int steps = 100;
        for (int i = 0; i <= steps; i++)
        {
            float t = i * 0.01f;
            Vector3 Linear = (1 - t) * startPosition + (t * endPosition);
            curvePath.Add(Linear);
        }

        StartCoroutine(PositionTransformLoop());
    }

    public void QuadraticTransformation(Vector3 endPosition)
    {
        Vector3 startPosition = this.transform.position;
        float maxHeight;
        float multiplier = 0.3f;

        double distance = Math.Sqrt(Math.Pow(startPosition.x - endPosition.x, 2) +
            Math.Pow(startPosition.z - endPosition.z,2));
        maxHeight = (float)distance * multiplier;
        maxHeight = Math.Max(0.5f, maxHeight);
        /*
         * Center point -> Pcenter = ((startPosition.x + endPosition.x)/2, maxHeight, (startPosition.z + endPosition.z)/2)
         * Linear Interpolation between points startPosition and CenterPosition, center and end point and then
         * between interpolations
         */
        Vector3 centerPosition = new((
            startPosition.x + endPosition.x) * 0.5f,
            maxHeight,
            (startPosition.z + endPosition.z) * 0.5f);

        int steps = (int)Math.Ceiling((double)distance) * 20;
        float stepsMultiplier = 1f / (float)steps;
        if(distance >= 4.0)
        {
            steps /= 2;
            stepsMultiplier *= 2f;
        }
        for (int i = 0; i <= steps; i++)
        {
            float t = i * stepsMultiplier;
            Vector3 Linear0 = (1 - t) * startPosition + (t * centerPosition);
            Vector3 Linear1 = (1 - t) * centerPosition + (t * endPosition);

            Vector3 Quadratic = (1 - t) * Linear0 + (t * Linear1);
            curvePath.Add(Quadratic);
        }

        StartCoroutine(PositionTransformLoop());
    }

    IEnumerator PositionTransformLoop()
    {
        finishedMoving = false;
        foreach (var c in curvePath)
        {
            yield return PositionTransformStep(c);
        }

        curvePath.Clear();
        finishedMoving = true;
    }

    IEnumerator PositionTransformStep(Vector3 quadratic)
    {
        transform.position = quadratic;
        promotionEffect.transform.position = quadratic;
        yield return new WaitForSecondsRealtime(0.001f);
    }
}