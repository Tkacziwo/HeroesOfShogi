using System;
using System.Xml.Schema;
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

        promotionEffect = Resources.Load("PromotionEffect") as GameObject;
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

    public void MovePiece(Vector2Int vec)
    {
        posX = vec.x;
        posY = vec.y;
    }

    public int[] GetMoveset()
        => Moveset;

    public string GetName()
        => pieceName;

    public Vector2Int GetPosition()
        => new(posX, posY);

    public Tuple<int, int> GetPositionTuple()
        => new(posX, posY);

    public Position GetPositionClass()
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

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, emptyGameObject.transform.position) > 0.001f)
        {
            var step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, emptyGameObject.transform.position, step);
            promotionEffect.transform.position = Vector3.MoveTowards(transform.position, emptyGameObject.transform.position, step);
        }
    }

}
