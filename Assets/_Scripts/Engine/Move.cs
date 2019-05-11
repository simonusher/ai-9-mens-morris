using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public Field fromField { get; }
    public Field toField { get; }

    public Move(Field fromField, Field toField)
    {
        this.fromField = fromField;
        this.toField = toField;
    }
}
