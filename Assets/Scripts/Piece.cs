using UnityEngine;
using UnityEngine.Rendering;

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

    private GameObject emptyGameObject;

    [SerializeField] private float speed;

    private bool isBlack;

    private bool isPromoted;

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
        pieceName = name;
        originalPieceName = name;
        Moveset = moveset;
        BackupOriginalMoveset();
        posX = x;
        posY = y;
        isSpecial = isSpecialPiece;
        isPromoted = false;
        isDrop = false;
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

    public Vector2Int GetPosition()
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

    public void SetTargetPosition(Vector3 target)
        => emptyGameObject.transform.position = target;

    public void Promote(int[] newMoveset)
    {
        Moveset = newMoveset;
        if (isBlack)
        {
            ReverseMovementMatrix();
        }
        isPromoted = true;
        //todo change textures
    }

    public void BackupOriginalMoveset()
    {
        originalMoveset = new int[Moveset.Length];
        for(int i = 0; i < Moveset.Length;i++)
        {
            originalMoveset[i] = Moveset[i];
        }
    }

    public void Demote()
    {
        pieceName = originalPieceName;
        Moveset = originalMoveset;
        isPromoted = false;
    }

    public bool GetIsDrop()
        => isDrop;

    public void SetIsDrop()
    {
        isDrop = true;

        //ReverseMovementMatrix();
    }

    public void ResetIsDrop()
        => isDrop = false;

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, emptyGameObject.transform.position) > 0.001f)
        {
            var step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, emptyGameObject.transform.position, step);
        }
    }

}
