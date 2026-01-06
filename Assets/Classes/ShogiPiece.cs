/// <summary>
/// Represents Shogi piece on board. Prepared to be cloned withing Minimax algorithm.
/// </summary>
public class ShogiPiece
{
    public string pieceName;

    protected string originalPieceName;

    protected int[] Moveset;

    protected int[] originalMoveset;

    protected Position pos;

    protected bool isSpecial;

    protected bool isDrop;

    public bool isKing;

    protected bool isBlack;

    protected bool isPromoted;

    public int value;

    public void InitPiece(string name, int[] moveset, int x, int y, bool isSpecial )
    {
        value = name switch
        {
            "Pawn" => 100,
            "GoldGeneral" => 500,
            "SilverGeneral" => 400,
            "Bishop" => 800,
            "Rook" => 800,
            "Lance" => 400,
            "Horse" => 300,
            "King" => 790,
            _ => 0,
        };
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
        pos = new(x, y);
        this.isSpecial = isSpecial;
        isPromoted = false;
        isDrop = false;
        if (pos.y < 3)
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

    public void MovePiece(Position p)
    {
        this.pos = new(p);
    }

    public int[] GetMoveset()
        => Moveset;

    public int[] GetOriginalMoveset()
        => originalMoveset;

    public Position GetPosition()
        => pos;
  
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

    public string GetName()
        => pieceName;

    public void Promote(int[] newMoveset)
    {
        Moveset = newMoveset;
        if (isBlack)
        {
            ReverseMovementMatrix();
        }
        isPromoted = true;
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
        pieceName = originalPieceName;
        Moveset = originalMoveset;
        isPromoted = false;
    }

    public bool GetIsDrop()
        => isDrop;

    public void SetIsDrop()
        => isDrop = true;
    public void ResetIsDrop()
        => isDrop = false;
}