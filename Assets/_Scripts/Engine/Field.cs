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
    public static int FIELD_INDEX_UNSET = -1;

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
        LastFieldIndex = FIELD_INDEX_UNSET;
    }
}
