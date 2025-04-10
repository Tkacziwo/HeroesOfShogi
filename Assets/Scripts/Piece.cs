using UnityEngine;

public class Piece : MonoBehaviour
{
    private string pieceName;

    private string originalPieceName;

    private int[] Moveset;

    private int posX;

    private int posY;

    private bool isSpecial;

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
        Moveset = moveset;
        posX = x;
        posY = y;
        isSpecial = isSpecialPiece;
        isPromoted = false;
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

    public bool GetIsSpecial()
        => isSpecial;

    public bool GetIsPromoted()
        => isPromoted;

    public void SetTargetPosition(Vector3 target)
        => emptyGameObject.transform.position = target;

    public void Promote(int[] newMoveset)
    {
        originalPieceName = pieceName;
        Moveset = newMoveset;
        isPromoted = true;
        //todo change textures
    }

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
