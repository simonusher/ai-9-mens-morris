using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public static int FIELD_UNSELECTED = -1;
    public static int FLYING_PAWNS_NUMBER = 3;
    public PlayerNumber PlayerNumber { get; private set; }
    public int PawnsToSet { get; private set; }
    public int PawnsLeft { get;  set; }

    public bool Flying {
        get
        {
            return PawnsLeft <= FLYING_PAWNS_NUMBER;
        }
    }

    public PlayerData(PlayerNumber playerNumber, int pawnsToSet)
    {
        this.PlayerNumber = playerNumber;
        PawnsToSet = pawnsToSet;
        PawnsLeft = 0;
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

    public bool HasPawnsLeft()
    {
        return PawnsLeft != 0;
    }

    public bool HasPawnsToSet()
    {
        return PawnsToSet != 0;
    }
}
