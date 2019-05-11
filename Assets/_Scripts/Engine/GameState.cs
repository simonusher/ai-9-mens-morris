using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    public Board Board { get; }
    public int FirstPlayerPawns { get; }
    public int SecondPlayerPawns { get;  }
    public PlayerNumber WinningPlayer { get; }

    public GameState(Board board, int firstPlayerPawns, int secondPlayerPawns, PlayerNumber winningPlayer)
    {
        Board = board;
        FirstPlayerPawns = firstPlayerPawns;
        SecondPlayerPawns = secondPlayerPawns;
        WinningPlayer = winningPlayer;
    }
}
