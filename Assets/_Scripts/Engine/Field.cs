using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field
{
    public Pawn Pawn { get; set; }

    public Field()
    {
        this.Pawn = new Pawn();
    }

    public Field(Field other)
    {
        this.Pawn = new Pawn(other.Pawn);
    }
}
