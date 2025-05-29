using System;

public class LogicPiece
{
    public string pieceName;

    private string originalPieceName;

    private int[] Moveset;

    private int[] originalMoveset;

    private Position pos;

    private bool isSpecial;

    private bool isDrop;

    private bool isBodyguard;

    public bool isKing;

    private bool isBlack;

    private bool isPromoted;

    public int value;

    public LogicPiece()
    {
    }

    public LogicPiece(Piece piece)
    {
        value = piece.GetName() switch
        {
            "Pawn" => 100,
            "GoldGeneral" => 500,
            "SilverGeneral" => 400,
            "Bishop" => 800,
            "Rook" => 800,
            "Lance" => 400,
            "Horse" => 300,
            "King" => 10000,
            _ => 0,
        };

        isKing = piece.isKing;
        originalPieceName = pieceName = piece.GetName();
        Moveset = piece.GetMoveset();
        BackupOriginalMoveset();
        pos = new(piece.GetPositionClass());
        isSpecial = piece.GetIsSpecial();
        isPromoted = piece.GetIsPromoted();
        isDrop = piece.GetIsDrop();
        isBlack = piece.GetIsBlack();
    }

    public LogicPiece(LogicPiece piece)
    {
        value = piece.pieceName switch
        {
            "Pawn" => 100,
            "GoldGeneral" => 500,
            "SilverGeneral" => 400,
            "Bishop" => 800,
            "Rook" => 800,
            "Lance" => 400,
            "Horse" => 300,
            "King" => 10000,
            _ => 0,
        };

        isKing = piece.isKing;
        originalPieceName = pieceName = piece.pieceName;
        Moveset = piece.GetMoveset();
        BackupOriginalMoveset();
        pos = new(piece.GetPosition());
        isSpecial = piece.GetIsSpecial();
        isPromoted = piece.GetIsPromoted();
        isDrop = piece.GetIsDrop();
        isBlack = piece.GetIsBlack();
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

    public void MovePiece(Tuple<int, int> pos)
        => this.pos = new(pos);
    
    public void MovePiece(Position p)
    {
        this.pos = new(p);
    }

    public int[] GetMoveset()
        => Moveset;

    public Tuple<int, int> GetPositionTuple()
        => new(pos.x, pos.y);

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
        //todo change textures
    }

    public void BackupOriginalMoveset()
    {
        originalMoveset = new int[Moveset.Length];
        for (int i = 0; i < Moveset.Length; i++)
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
        => isDrop = true;
    public void ResetIsDrop()
        => isDrop = false;

    public bool GetIsBodyguard()
        => isBodyguard;

    public bool SetIsBodyguard()
        => isBodyguard = true;

    public bool ResetIsBodyguard()
        => isBodyguard = false;

    public int GetValue()
        => value;
}