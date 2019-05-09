using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerNumber
{
    FirstPlayer,
    SecondPlayer,
    None
}

public class Field
{
    public PlayerNumber PawnPlayerNumber { get; set; }
    public int LastFieldIndex { get; set; }

    public Field()
    {
        Reset();
    }

    public Field(Field other)
    {
        PawnPlayerNumber = other.PawnPlayerNumber;
        LastFieldIndex = other.LastFieldIndex;
    }

    public void Reset()
    {
        PawnPlayerNumber = PlayerNumber.None;
        LastFieldIndex = -1;
    }
}
