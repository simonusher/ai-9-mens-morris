using System;
using System.Collections;
using System.Collections.Generic;
public enum PlayerNumber
{
    First,
    Second
}

public enum MillGameStage
{
    PlacingPawns,
    NormalPlay,
    ThreePawnsLeft
}

public class GameEngine
{
    private static int POSSIBLE_MILLS = 16;
    private List<Mill> mills;
    public Board currentBoard { get; private set; }
    private Player firstPlayer;
    private Player secondPlayer;

    public GameEngine(PlayerType firstPlayerType, PlayerType secondPlayerType)
    {
        currentBoard = new Board();
        InitializeMills();
        InitializePlayers(firstPlayerType, secondPlayerType);
    }

    public List<Board> GetPossibleMoves(PlayerNumber playerNumber, Board previousBoard)
    {
        //TODO
        return null;
    }

    private void InitializePlayers(PlayerType firstPlayerType, PlayerType secondPlayerType)
    {
        //TODO
    }

    private void InitializeMills()
    {
        mills = new List<Mill>(POSSIBLE_MILLS);
        mills.Add(new Mill(0, 1, 2));
        mills.Add(new Mill(3, 4, 5));
        mills.Add(new Mill(6, 7, 8));
        mills.Add(new Mill(9, 10, 11));
        mills.Add(new Mill(12, 13, 14));
        mills.Add(new Mill(15, 16, 17));
        mills.Add(new Mill(18, 19, 20));
        mills.Add(new Mill(21, 22, 23));
        mills.Add(new Mill(0, 9, 21));
        mills.Add(new Mill(3, 10, 18));
        mills.Add(new Mill(6, 11, 15));
        mills.Add(new Mill(1, 4, 7));
        mills.Add(new Mill(16, 19, 22));
        mills.Add(new Mill(8, 12, 17));
        mills.Add(new Mill(5, 13, 20));
        mills.Add(new Mill(2, 14, 23));
    }
}
