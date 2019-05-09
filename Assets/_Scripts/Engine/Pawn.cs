using System.Collections;
using System.Collections.Generic;

public enum PawnType
{
    FirstPlayer,
    SecondPlayer,
    None
}

public class Pawn
{
    public PawnType PawnType { get; set; }
    public int LastFieldIndex { get; set; }

    public Pawn()
    {
        PawnType = PawnType.None;
        LastFieldIndex = -1;
    }

    public Pawn(Pawn other)
    {
        this.PawnType = other.PawnType;
        this.LastFieldIndex = other.LastFieldIndex;
    }
}
