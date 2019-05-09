using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType
{
    Human,
    AI
}

public abstract class Player
{
    private GameEngine game;
    private PlayerNumber playerNumber;

    public Player(GameEngine game, PlayerNumber playerNumber)
    {
        this.game = game;
        this.playerNumber = playerNumber;
    }
    public abstract Board MakeMove();
}
