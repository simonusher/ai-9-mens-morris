using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public static int FIELD_UNSELECTED = -1;
    public static int FLYING_PAWNS_NUMBER = 3;
    public PlayerNumber playerNumber { get; private set; }
    public int PawnsToSet { get; private set; }
    public int PawnsLeft { get; private set; }
    public int SelectedFieldIndex { get; private set; }

    public Player(PlayerNumber playerNumber, int pawnsToSet)
    {
        this.playerNumber = playerNumber;
        PawnsToSet = pawnsToSet;
        PawnsLeft = 0;
        SelectedFieldIndex = FIELD_UNSELECTED;
    }

    public void SetPawn()
    {
        if(PawnsToSet > 0)
        {
            PawnsToSet--;
            PawnsLeft++;
        }
    }

    public void RemovePawn()
    {
        PawnsLeft--;
    }

    public void SelectPawn(int fieldIndex)
    {
        SelectedFieldIndex = fieldIndex;
    }

    public void DeselectField()
    {
        SelectedFieldIndex = FIELD_UNSELECTED;
    }

    public bool HasPawnsLeft()
    {
        return PawnsLeft != 0;
    }

    public bool HasPawnsToSet()
    {
        return PawnsToSet != 0;
    }

    public bool IsFlying()
    {
        return PawnsLeft <= FLYING_PAWNS_NUMBER;
    }
}
