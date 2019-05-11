using System;
using System.Collections;
using System.Collections.Generic;


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
    private static int LOSING_PAWNS_NUMBER_THRESHOLD = 2;

    public delegate void GameFinished(PlayerNumber winningPlayerNumber);
    public event GameFinished OnGameFinished = delegate { };

    public delegate void BoardChanged(Board newBoard);
    public event BoardChanged OnBoardChanged = delegate { };

    public delegate void PlayerTurnChanged(PlayerNumber currentMovingPlayerNumber);
    public event PlayerTurnChanged OnPlayerTurnChanged = delegate { };

    private PlayerNumber _winningPlayerNumber;
    public PlayerNumber WinningPlayerNumber {
        get
        {
            return _winningPlayerNumber;
        }
        private set
        {
            _winningPlayerNumber = value;
            OnGameFinished(_winningPlayerNumber);
        }
    }

    private bool shouldLogToFile;
    private Mill[] mills;
    private List<List<int>> possibleMoveIndices;
    private string[] fieldNames;

    public Board currentBoard { get; private set; }

    private MillGameStage millGameStage;
    

    private Player firstPlayer;
    private Player secondPlayer;

    private PlayerNumber _currentMovingPlayerNumber;
    public PlayerNumber CurrentMovingPlayerNumber
    {
        get
        {
            return _currentMovingPlayerNumber;
        }
        private set
        {
            _currentMovingPlayerNumber = value;
            OnPlayerTurnChanged(_currentMovingPlayerNumber);
        }
    }


    private Field lastSelectedField;

    
    private int pawnsToRemove;
    private HashSet<Mill> lastTurnActiveMills;

    public GameEngine(bool shouldLogToFile)
    {
        InitializeMills();
        InitializePossibleMoveIndices();
        InitializeFieldNames();
        InitializeBoard();
        InitializePlayers();
        millGameStage = MillGameStage.PlacingPawns;
        pawnsToRemove = 0;
        lastSelectedField = null;
        lastTurnActiveMills = new HashSet<Mill>();
        _winningPlayerNumber = PlayerNumber.None;
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
            HandlePawnMoving(fieldIndex);
        }
        CheckGameStateChange();
    }

    private void HandlePawnRemoval(int fieldIndex)
    {
        Field field = currentBoard.GetField(fieldIndex);
        if(field.PawnPlayerNumber != PlayerNumber.None)
        {
            if(field.PawnPlayerNumber != CurrentMovingPlayerNumber)
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
        lastTurnActiveMills = GetActiveMills(currentBoard);
        if(pawnsToRemove == 0)
        {
            SwitchPlayer();
        }
    }

    private void HandlePawnPlacing(int fieldIndex)
    {
        if (currentBoard.GetField(fieldIndex).PawnPlayerNumber == PlayerNumber.None)
        {
            currentBoard.GetField(fieldIndex).PawnPlayerNumber = CurrentMovingPlayerNumber;
            getCurrentlyMovingPlayer().SetPawn();
            TogglePawnDeletingOrSwitchPlayer();
            NotifyBoardChanged();
        }
    }

    private void TogglePawnDeletingOrSwitchPlayer()
    {
        HashSet<Mill> activeMills = GetActiveMills(currentBoard);
        HashSet<Mill> newActiveMills = new HashSet<Mill>(activeMills);
        newActiveMills.ExceptWith(lastTurnActiveMills);
        if (newActiveMills.Count > 0)
        {
            pawnsToRemove = newActiveMills.Count;
            lastTurnActiveMills = activeMills;
        }
        else
        {
            SwitchPlayer();
        }
    }

    private void HandlePawnMoving(int fieldIndex)
    {
        Player currentPlayer = getCurrentlyMovingPlayer();
        Field newField = currentBoard.GetField(fieldIndex);
        PlayerNumber selectedFieldPawnPlayer = newField.PawnPlayerNumber;
        if (selectedFieldPawnPlayer == currentPlayer.PlayerNumber)
        {
            lastSelectedField = newField;
        }
        else if(lastSelectedField != null && selectedFieldPawnPlayer == PlayerNumber.None)
        {
            if (currentPlayer.Flying)
            {
                HandleFlying(newField);
            } else
            {
                HandleNormalMove(newField);
            }
        }
    }

    private void HandleNormalMove(Field newField)
    {
        List<Field> possibleNewFields = GetPossibleNewFields(lastSelectedField.FieldIndex, currentBoard);
        if(possibleNewFields.Contains(newField) && lastSelectedField.CanMoveTo(newField))
        {
            PerformSelectedMove(newField);
        }
    }

    private List<Field> GetPossibleNewFields(int fromIndex, Board board)
    {
        List<Field> possibleFields = new List<Field>();
        foreach (int index in possibleMoveIndices[fromIndex])
        {
            possibleFields.Add(board.GetField(index));
        }
        return possibleFields;
    }

    private void HandleFlying(Field newField)
    {
        if (lastSelectedField.CanMoveTo(newField))
        {
            PerformSelectedMove(newField);
        }
    }

    private void PerformSelectedMove(Field newField)
    {
        lastSelectedField.MoveTo(newField);
        NotifyBoardChanged();
        TogglePawnDeletingOrSwitchPlayer();
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
        else
        {
            if(firstPlayer.PawnsLeft <= LOSING_PAWNS_NUMBER_THRESHOLD)
            {
                OnGameFinished(PlayerNumber.SecondPlayer);
            }
            else if(secondPlayer.PawnsLeft <= LOSING_PAWNS_NUMBER_THRESHOLD)
            {
                OnGameFinished(PlayerNumber.FirstPlayer);
            }
        }
    }

    private void NotifyBoardChanged()
    {
        OnBoardChanged(currentBoard);
    }

    private void SwitchPlayer()
    {
        if(CurrentMovingPlayerNumber == PlayerNumber.FirstPlayer)
        {
            CurrentMovingPlayerNumber = PlayerNumber.SecondPlayer;
        } else
        {
            CurrentMovingPlayerNumber = PlayerNumber.FirstPlayer;
        }
        lastSelectedField = null;
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
        if(CurrentMovingPlayerNumber == PlayerNumber.FirstPlayer) {
            return firstPlayer;
        }
        return secondPlayer;
    }

    private Player getOtherPlayer()
    {
        if (CurrentMovingPlayerNumber == PlayerNumber.FirstPlayer)
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

    private void InitializePossibleMoveIndices()
    {
        possibleMoveIndices = new List<List<int>>(Board.DEFAULT_NUMBER_OF_FIELDS);
        possibleMoveIndices.Add(new List<int> { 1, 9 });
        possibleMoveIndices.Add(new List<int> { 0, 2, 4 });
        possibleMoveIndices.Add(new List<int> { 1, 14 });
        possibleMoveIndices.Add(new List<int> { 4, 10 });
        possibleMoveIndices.Add(new List<int> { 1, 3, 5, 7});
        possibleMoveIndices.Add(new List<int> { 4, 13 });
        possibleMoveIndices.Add(new List<int> { 7, 11 });
        possibleMoveIndices.Add(new List<int> { 4, 6, 8 });
        possibleMoveIndices.Add(new List<int> { 7, 12 });
        possibleMoveIndices.Add(new List<int> { 0, 10, 21 });
        possibleMoveIndices.Add(new List<int> { 3, 9, 11, 18 });
        possibleMoveIndices.Add(new List<int> { 6, 10, 15 });
        possibleMoveIndices.Add(new List<int> { 8, 13, 17 });
        possibleMoveIndices.Add(new List<int> { 5, 12, 14, 20 });
        possibleMoveIndices.Add(new List<int> { 2, 13, 23 });
        possibleMoveIndices.Add(new List<int> { 11, 16 });
        possibleMoveIndices.Add(new List<int> { 15, 17, 19 });
        possibleMoveIndices.Add(new List<int> { 12, 16 });
        possibleMoveIndices.Add(new List<int> { 10, 19 });
        possibleMoveIndices.Add(new List<int> { 16, 18, 20, 22 });
        possibleMoveIndices.Add(new List<int> { 13, 19 });
        possibleMoveIndices.Add(new List<int> { 9, 22 });
        possibleMoveIndices.Add(new List<int> { 19, 21, 23 });
        possibleMoveIndices.Add(new List<int> { 14, 22 });
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

    private HashSet<Mill> GetActiveMills(Board board)
    {
        HashSet<Mill> activeMills = new HashSet<Mill>();
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
                    activeMills.Add(mill);
                }
            }
        }

        return activeMills;
    }
}
