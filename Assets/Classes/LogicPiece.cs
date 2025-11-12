using System;
using System.Xml.Linq;
using UnityEngine;

/// <summary>
/// Logic counterpart to Piece class. Behaves the same.
/// </summary>
public class LogicPiece
{
    public string pieceName;

    protected string originalPieceName;

    protected int[] Moveset;

    protected int[] originalMoveset;

    protected Position pos;

    protected bool isSpecial;

    protected bool isDrop;

    protected bool isBodyguard;

    public bool isKing;

    protected bool isBlack;

    protected bool isPromoted;

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
            "King" => 790,
            _ => 0,
        };

        isKing = piece.isKing;
        originalPieceName = pieceName = piece.GetName();
        Moveset = piece.GetMoveset();
        originalMoveset = piece.GetOriginalMovest();
        pos = new(piece.GetPosition());
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
        Moveset = piece.Moveset;
        originalMoveset = piece.originalMoveset;
        pos = piece.pos;
        isSpecial = piece.isSpecial;
        isPromoted = piece.isPromoted;
        isDrop = piece.isDrop;
        isBlack = piece.isBlack;
    }

    public void InitPiece(string name, int[] moveset, int x, int y, bool isSpecial )
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

        //promotionEffect = Resources.Load("Prefabs/ParticleEffects/PromotionEffect") as GameObject;
        //promotionEffect = Instantiate(promotionEffect);
        //promotionEffect.GetComponent<ParticleSystem>().Stop();
        //promotionEffect.GetComponent<ParticleSystem>().Clear();
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
        isBodyguard = false;
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
        this.pos = new(p);
    }

    public int[] GetMoveset()
        => Moveset;

    public int[] GetOriginalMoveset()
        => originalMoveset;

    public Position GetPosition()
        => pos;

    public void SetPosition(Position p)
        => pos = new(p);

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

    public bool GetIsBodyguard()
        => isBodyguard;

    public bool SetIsBodyguard()
        => isBodyguard = true;

    public bool ResetIsBodyguard()
        => isBodyguard = false;

    public int GetValue()
        => value;
}