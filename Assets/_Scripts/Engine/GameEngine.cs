using System;
using System.Collections;
using System.Collections.Generic;


public enum GameType
{
    PlayerVsAi,
    AiVsAi
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
    private static int PLAYERS_PAWNS = 9;
    private Mill[] mills;
    private string[] fieldNames;

    private bool shouldLogToFile;
    public Board currentBoard { get; private set; }

    private MillGameStage millGameStage;

    private bool gameFinished;

    public delegate void BoardChanged(Board newBoard);
    public event BoardChanged OnBoardChanged = delegate { };


    private Player firstPlayer;
    private Player secondPlayer;
    private PlayerNumber currentMovingPlayerNumber;

    private int selectedFieldIndex;

    private int pawnsToRemove;

    private int lastTurnMills;

    public GameEngine(bool shouldLogToFile)
    {
        InitializeMills();
        InitializeFieldNames();
        InitializeBoard();
        InitializePlayers();
        millGameStage = MillGameStage.PlacingPawns;
        gameFinished = false;
        pawnsToRemove = 0;
        lastTurnMills = 0;
        selectedFieldIndex = Field.FIELD_INDEX_UNSET;
    }

    private void InitializePlayers()
    {
        firstPlayer = new Player(PlayerNumber.FirstPlayer, PLAYERS_PAWNS);
        secondPlayer = new Player(PlayerNumber.SecondPlayer, PLAYERS_PAWNS);
    }

    public void HandleSelection(int fieldIndex)
    {
        if (pawnsToRemove > 0)
        {
            HandlePawnRemoval(fieldIndex);
        }
        else if(millGameStage == MillGameStage.PlacingPawns)
        {
            HandlePawnPlacing(fieldIndex);
        } else
        {

        }
        CheckGameStateChange();
    }

    private void HandlePawnRemoval(int fieldIndex)
    {
        Field field = currentBoard.GetField(fieldIndex);
        if(field.PawnPlayerNumber != PlayerNumber.None)
        {
            if(field.PawnPlayerNumber != currentMovingPlayerNumber)
            {
                RemovePawn(field);
            }
        }
    }

    private void RemovePawn(Field field)
    {
        field.Reset();
        getOtherPlayer().RemovePawn();
        NotifyBoardChanged();
        pawnsToRemove--;
        if(pawnsToRemove == 0)
        {
            SwitchPlayer();
        }
    }
    
    private void HandlePawnPlacing(int fieldIndex)
    {
        if (currentBoard.GetField(fieldIndex).PawnPlayerNumber == PlayerNumber.None)
        {
            currentBoard.GetField(fieldIndex).PawnPlayerNumber = currentMovingPlayerNumber;
            getCurrentlyMovingPlayer().SetPawn();
            int millsNumber = CountMills(currentBoard);
            if (millsNumber > lastTurnMills)
            {
                pawnsToRemove = millsNumber - lastTurnMills;
                lastTurnMills = millsNumber;
            }
            else
            {
                SwitchPlayer();
            }
            NotifyBoardChanged();
        }
    }

    private void HandlePawnMoving(int fieldIndex)
    {
        //TODO
    }

    private void CheckGameStateChange()
    {
        if(millGameStage == MillGameStage.PlacingPawns)
        {
            if(!firstPlayer.HasPawnsToSet() && !secondPlayer.HasPawnsToSet())
            {
                millGameStage = MillGameStage.NormalPlay;
            }
        }
    }

    private void NotifyBoardChanged()
    {
        OnBoardChanged(currentBoard);
    }

    private void SwitchPlayer()
    {
        if(currentMovingPlayerNumber == PlayerNumber.FirstPlayer)
        {
            currentMovingPlayerNumber = PlayerNumber.SecondPlayer;
        } else
        {
            currentMovingPlayerNumber = PlayerNumber.FirstPlayer;
        }
    }

    public void MakeMove(Board board)
    {

    }

    private void InitializeBoard()
    {
        currentBoard = new Board();
    }

    public List<Board> GetPossibleMoves(PlayerNumber playerNumber, Board previousBoard)
    {
        //TODO
        return null;
    }

    private Player getCurrentlyMovingPlayer()
    {
        if(currentMovingPlayerNumber == PlayerNumber.FirstPlayer) {
            return firstPlayer;
        }
        return secondPlayer;
    }

    private Player getOtherPlayer()
    {
        if (currentMovingPlayerNumber == PlayerNumber.FirstPlayer)
        {
            return secondPlayer;
        }
        return firstPlayer;
    }


    private void InitializeMills()
    {
        int i = 0;
        mills = new Mill[POSSIBLE_MILLS];
        mills[i++] = (new Mill(0, 1, 2));
        mills[i++] = (new Mill(3, 4, 5));
        mills[i++] = (new Mill(6, 7, 8));
        mills[i++] = (new Mill(9, 10, 11));
        mills[i++] = (new Mill(12, 13, 14));
        mills[i++] = (new Mill(15, 16, 17));
        mills[i++] = (new Mill(18, 19, 20));
        mills[i++] = (new Mill(21, 22, 23));
        mills[i++] = (new Mill(0, 9, 21));
        mills[i++] = (new Mill(3, 10, 18));
        mills[i++] = (new Mill(6, 11, 15));
        mills[i++] = (new Mill(1, 4, 7));
        mills[i++] = (new Mill(16, 19, 22));
        mills[i++] = (new Mill(8, 12, 17));
        mills[i++] = (new Mill(5, 13, 20));
        mills[i++] = (new Mill(2, 14, 23));
    }

    private void InitializeFieldNames()
    {
        fieldNames = new string[Board.DEFAULT_NUMBER_OF_FIELDS];
        fieldNames[0] = "A1";
        fieldNames[1] = "D1";
        fieldNames[2] = "G1";
        fieldNames[3] = "B2";
        fieldNames[4] = "D2";
        fieldNames[5] = "F2";
        fieldNames[6] = "C3";
        fieldNames[7] = "D3";
        fieldNames[8] = "E3";
        fieldNames[9] = "A4";
        fieldNames[10] = "B4";
        fieldNames[11] = "C4";
        fieldNames[12] = "E4";
        fieldNames[13] = "F4";
        fieldNames[14] = "G4";
        fieldNames[15] = "C5";
        fieldNames[16] = "D5";
        fieldNames[17] = "E5";
        fieldNames[18] = "B6";
        fieldNames[19] = "D6";
        fieldNames[20] = "F6";
        fieldNames[21] = "A7";
        fieldNames[22] = "D7";
        fieldNames[23] = "G7";
    }

    private int CountMills(Board board)
    {
        int boardMills = 0;
        Mill mill;
        for (int i = 0; i < mills.Length; i++)
        {
            mill = mills[i];
            PlayerNumber playerMillNumber = board.GetField(mill.MillIndices[0]).PawnPlayerNumber;
            if (playerMillNumber != PlayerNumber.None)
            {
                bool millPossible = true;
                for (int j = 1; j < mill.MillIndices.Count && millPossible; j++)
                {
                    if (playerMillNumber != board.GetField(mill.MillIndices[j]).PawnPlayerNumber)
                    {
                        millPossible = false;
                    }
                }

                if (millPossible)
                {
                    boardMills++;
                }
            }
        }

        return boardMills;
    }
}
