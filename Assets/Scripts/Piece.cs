using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Class which symbolizes playable piece on board.
/// </summary>
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

    /// <summary>
    /// Initializes piece with passed values.
    /// </summary>
    /// <param name="name">Name of piece</param>
    /// <param name="moveset">Possible moveset</param>
    /// <param name="x">x position</param>
    /// <param name="y">y position</param>
    /// <param name="isSpecialPiece">Whether is special or standard piece</param>
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

    /// <summary>
    /// Reverses movement matrix, when killed by enemy.
    /// </summary>
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

    /// <summary>
    /// Moves piece to destination.
    /// </summary>
    /// <param name="p">destination position</param>
    public void MovePiece(Position p)
    {
        posX = p.x;
        posY = p.y;
    }

    /// <summary>
    /// Gets piece moveset.
    /// </summary>
    /// <returns>int[]</returns>
    public int[] GetMoveset()
        => Moveset;

    /// <summary>
    /// Gets original moveset before promotion.
    /// </summary>
    /// <returns>int[]</returns>
    public int[] GetOriginalMovest()
        => originalMoveset;

    /// <summary>
    /// Gets piece name.
    /// </summary>
    /// <returns>string</returns>
    public string GetName()
        => pieceName;

    /// <summary>
    /// Gets current piece position.
    /// </summary>
    /// <returns>Position</returns>
    public Position GetPosition()
        => new(posX, posY);

    /// <summary>
    /// Returns true when piece is black. Otherwise false.
    /// </summary>
    /// <returns>bool</returns>
    public bool GetIsBlack()
        => isBlack;

    /// <summary>
    /// Sets isBlack value to true.
    /// </summary>
    public void SetIsBlack()
        => isBlack = true;

    /// <summary>
    /// Sets isBlack value to false.
    /// </summary>
    public void ResetIsBlack()
        => isBlack = false;

    /// <summary>
    /// Returns true if piece is special. Otherwise false.
    /// </summary>
    /// <returns>bool</returns>
    public bool GetIsSpecial()
        => isSpecial;

    /// <summary>
    /// Returns true when piece is promoted. Otherwise false.
    /// </summary>
    /// <returns>bool</returns>
    public bool GetIsPromoted()
        => isPromoted;

    /// <summary>
    /// Immediately changes position of piece
    /// </summary>
    /// <param name="target">target position</param>
    public void SetPiecePositionImmediate(Vector3 target)
    {
        transform.position = target;
        emptyGameObject.transform.position = target;
    }

    /// <summary>
    /// Promotes piece
    /// </summary>
    /// <param name="newMoveset"></param>
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
    }

    /// <summary>
    /// Backups original moveset to class field.
    /// </summary>
    public void BackupOriginalMoveset(int[] moveset)
    {
        originalMoveset = new int[moveset.Length];
        for (int i = 0; i < moveset.Length; i++)
        {
            originalMoveset[i] = moveset[i];
        }
    }

    /// <summary>
    /// Takes away piece promotion. Restores default moveset.
    /// </summary>
    public void Demote()
    {
        promotionEffect.GetComponent<ParticleSystem>().Stop();
        pieceName = new(originalPieceName);
        Moveset = originalMoveset;
        isPromoted = false;
    }

    /// <summary>
    /// Returns true when piece is drop. False otherwise.
    /// </summary>
    /// <returns>bool</returns>
    public bool GetIsDrop()
        => isDrop;

    /// <summary>
    /// Sets isDrop value to true.
    /// </summary>
    public void SetIsDrop()
    {
        isDrop = true;
    }

    /// <summary>
    /// Sets isDrop value to false.
    /// </summary>
    public void ResetIsDrop()
        => isDrop = false;

    /// <summary>
    /// Creates linear transformation between piece position and endPosition parameter to accomplish smooth movement.
    /// </summary>
    /// <param name="endPosition">Target position</param>
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

    /// <summary>
    /// Creates 2 linear transformation from beginning to center and center to end. Then creates linear transformation between created
    /// linear transformation to achieve quadratic transformation on a curve.
    /// Automatically calculates height and center point of transformation to make longer movements have higher point.
    /// </summary>
    /// <param name="endPosition">Target position</param>
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

    /// <summary>
    /// Loop for moving piece
    /// </summary>
    /// <returns>IEnumerator</returns>
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

    /// <summary>
    /// Executes single step in transformation and sets piece position.
    /// </summary>
    /// <param name="quadratic">quadratic vector3 step coordinates</param>
    /// <returns>IEnumerator</returns>
    IEnumerator PositionTransformStep(Vector3 quadratic)
    {
        transform.position = quadratic;
        promotionEffect.transform.position = quadratic;
        yield return new WaitForSecondsRealtime(0.001f);
    }
}