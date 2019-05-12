using System;
using System.Collections;
using System.Collections.Generic;


public enum MillGameStage
{
    PlacingPawns,
    NormalPlay
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

    private Board _currentBoard;
    public Board currentBoard {
        get {
            return _currentBoard;
        }
        private set {
            _currentBoard = value;
            NotifyBoardChanged();
        }
    }

    private MillGameStage millGameStage;
    

    private PlayerData firstPlayer;
    private PlayerData secondPlayer;

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
        firstPlayer = new PlayerData(PlayerNumber.FirstPlayer, PLAYERS_PAWNS);
        secondPlayer = new PlayerData(PlayerNumber.SecondPlayer, PLAYERS_PAWNS);
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
        MillDifference millDifference = GetMillDifference(lastTurnActiveMills, currentBoard);
        if (millDifference.NewMills.Count > 0)
        {
            pawnsToRemove = millDifference.NewMills.Count;
            lastTurnActiveMills = millDifference.TurnActiveMills;
        }
        else
        {
            SwitchPlayer();
        }
    }

    private void HandlePawnMoving(int fieldIndex)
    {
        PlayerData currentPlayer = getCurrentlyMovingPlayer();
        Field newField = currentBoard.GetField(fieldIndex);
        PlayerNumber selectedFieldPawnPlayer = newField.PawnPlayerNumber;
        if (selectedFieldPawnPlayer == currentPlayer.PlayerNumber)
        {
            lastSelectedField = newField;
        }
        else if(lastSelectedField != null && selectedFieldPawnPlayer == PlayerNumber.None)
        {
            int currentPlayersPawnsLeft = currentBoard.GetPlayerFields(currentPlayer.PlayerNumber).Count;
            if(currentPlayersPawnsLeft <= PlayerData.FLYING_PAWNS_NUMBER)
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
        if(possibleNewFields.Contains(newField))
        {
            PerformSelectedMove(newField);
        }
    }

    private List<Field> GetPossibleNewFields(int fromIndex, Board board)
    {
        return GetPossibleNewFields(board.GetField(fromIndex), board);
    }

    private List<Field> GetPossibleNewFields(Field fromField, Board board)
    {
        List<Field> possibleFields = new List<Field>();
        foreach (int index in possibleMoveIndices[fromField.FieldIndex])
        {
            Field toField = board.GetField(index);
            if (fromField.CanMoveTo(toField))
            {
                possibleFields.Add(board.GetField(index));
            }
        }
        return possibleFields;
    }

    private List<Field> GetPossibleNewFieldsFlying(Field fromField, Board board)
    {
        List<Field> possibleFields = new List<Field>();
        foreach(var toField in board.Fields)
        {
            if (fromField.CanMoveTo(toField))
            {
                possibleFields.Add(toField);
            }
        }
        return possibleFields;
    }

    private List<Move> GetAllPossibleMoves(PlayerNumber playerNumber, Board board)
    {
        List<Move> allMoves = new List<Move>();
        List<Field> playersFields = board.GetPlayerFields(playerNumber);
        int playerPawns = playersFields.Count;
        if (playerPawns <= PlayerData.FLYING_PAWNS_NUMBER)
        {
            foreach (var fromField in playersFields)
            {
                List<Field> toFields = GetPossibleNewFieldsFlying(fromField, board);
                foreach (var toField in toFields)
                {
                    allMoves.Add(new Move(fromField, toField));
                }
            }
        } else
        {
            foreach (var fromField in playersFields)
            {
                List<Field> toFields = GetPossibleNewFields(fromField, board);
                foreach (var toField in toFields)
                {
                    allMoves.Add(new Move(fromField, toField));
                }
            }
        }
        
        return allMoves;
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
                CheckGameStateChange();
            }
        }
        else
        {
            CheckGameFinished(currentBoard);
        }
    }

    private void CheckGameFinished(Board board)
    {
        PlayerNumber winningPlayer = GetWinningPlayer(board);
        if(winningPlayer != PlayerNumber.None)
        {
            OnGameFinished(winningPlayer);
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

    public void MakeMove(GameState gameState)
    {
        if(gameState != null)
        {
            if (millGameStage == MillGameStage.PlacingPawns)
            {
                getCurrentlyMovingPlayer().SetPawn();
            }
            currentBoard = gameState.Board;
            lastTurnActiveMills = GetActiveMills(currentBoard);
            SwitchPlayer();
        }
        CheckGameStateChange();
        //else
        //{
        //    WinningPlayerNumber = getOtherPlayer().PlayerNumber;
        //}
    }

    private void InitializeBoard()
    {
        currentBoard = new Board();
    }

    public List<GameState> GetAllPossibleNextStates(PlayerNumber playerNumber, GameState previousState)
    {
        Board previousBoard = previousState.Board;
        HashSet<Mill> previousActiveMills = GetActiveMills(previousBoard);
        PlayerNumber otherPlayerNumber = playerNumber == PlayerNumber.FirstPlayer ? PlayerNumber.SecondPlayer : PlayerNumber.FirstPlayer;
        List<GameState> gameStates = new List<GameState>();
        if (millGameStage == MillGameStage.PlacingPawns)
        {
            List<Field> emptyFields = previousBoard.GetEmptyFields();
            foreach (var emptyField in emptyFields)
            {
                Board newBoard = new Board(previousBoard);
                newBoard.GetField(emptyField.FieldIndex).PawnPlayerNumber = playerNumber;
                MillDifference millDifference = GetMillDifference(previousActiveMills, newBoard);
                if (millDifference.NewMills.Count == 0)
                {
                    gameStates.Add(GetGameState(newBoard));
                }
                else if (millDifference.NewMills.Count == 1)
                {
                    List<Field> otherPlayersFields = newBoard.GetPlayerFields(otherPlayerNumber);
                    foreach (var otherPlayerField in otherPlayersFields)
                    {
                        Board boardWithRemovedPawn = new Board(newBoard);
                        boardWithRemovedPawn.GetField(otherPlayerField.FieldIndex).Reset();
                        gameStates.Add(GetGameState(boardWithRemovedPawn));
                    }
                }
                else
                {
                    List<Field> otherPlayersFields = newBoard.GetPlayerFields(otherPlayerNumber);
                    for (int i = 0; i < otherPlayersFields.Count - 1; i++)
                    {
                        Board boardWithRemovedPawn = new Board(newBoard);
                        boardWithRemovedPawn.GetField(otherPlayersFields[i].FieldIndex).Reset();
                        for (int j = i + 1; j < otherPlayersFields.Count; j++)
                        {
                            Board boardWithSecondRemovedPawn = new Board(newBoard);
                            boardWithSecondRemovedPawn.GetField(otherPlayersFields[j].FieldIndex).Reset();
                            gameStates.Add(GetGameState(boardWithSecondRemovedPawn));
                        }
                    }
                }
            }
            return gameStates;
        }
        else
        {
            List<Move> possibleMoves = GetAllPossibleMoves(playerNumber, previousBoard);
            foreach (var move in possibleMoves)
            {
                Board newBoard = new Board(previousBoard);
                newBoard.GetField(move.fromField.FieldIndex).MoveTo(newBoard.GetField(move.toField.FieldIndex));
                MillDifference millDifference = GetMillDifference(previousActiveMills, newBoard);
                if (millDifference.NewMills.Count == 0)
                {
                    gameStates.Add(GetGameState(newBoard));
                }
                else if (millDifference.NewMills.Count == 1)
                {
                    List<Field> otherPlayersFields = newBoard.GetPlayerFields(otherPlayerNumber);
                    foreach (var otherPlayerField in otherPlayersFields)
                    {
                        Board boardWithRemovedPawn = new Board(newBoard);
                        boardWithRemovedPawn.GetField(otherPlayerField.FieldIndex).Reset();
                        gameStates.Add(GetGameState(boardWithRemovedPawn));
                    }
                }
            }
        }
        return gameStates;
    }

    private GameState GetGameState(Board board)
    {
        int firstPlayersPawns = board.GetPlayerFields(PlayerNumber.FirstPlayer).Count;
        int secondPlayerPawns = board.GetPlayerFields(PlayerNumber.SecondPlayer).Count;
        PlayerNumber winningPlayer = GetWinningPlayer(board);
        return new GameState(board, firstPlayersPawns, secondPlayerPawns, winningPlayer);
    }

    private PlayerNumber GetWinningPlayer(Board board)
    {
        int firstPlayersPawns = board.GetPlayerFields(PlayerNumber.FirstPlayer).Count;
        int secondPlayerPawns = board.GetPlayerFields(PlayerNumber.SecondPlayer).Count;
        int firstPlayersPossibleMoves = GetAllPossibleMoves(PlayerNumber.FirstPlayer, board).Count;
        int secondPlayerPossibleMoves = GetAllPossibleMoves(PlayerNumber.SecondPlayer, board).Count;
        if (firstPlayersPawns <= LOSING_PAWNS_NUMBER_THRESHOLD || firstPlayersPossibleMoves == 0)
        {
            return PlayerNumber.SecondPlayer;
        } else if (secondPlayerPawns <= LOSING_PAWNS_NUMBER_THRESHOLD || secondPlayerPossibleMoves == 0)
        {
            return PlayerNumber.FirstPlayer;
        } else
        {
            return PlayerNumber.None;
        }
    }

    public MillDifference GetMillDifference(HashSet<Mill> previousMills, Board board)
    {
        HashSet<Mill> activeMills = GetActiveMills(board);
        HashSet<Mill> newActiveMills = new HashSet<Mill>(activeMills);
        newActiveMills.ExceptWith(previousMills);
        return new MillDifference(activeMills, newActiveMills);
    }

    public GameState GetCurrentGameState()
    {
        int firstPlayerPawnsLeft = _currentBoard.GetPlayerFields(PlayerNumber.FirstPlayer).Count;
        int secondPlayerPawnsLeft = _currentBoard.GetPlayerFields(PlayerNumber.SecondPlayer).Count;
        return new GameState(currentBoard, firstPlayerPawnsLeft, secondPlayerPawnsLeft, _winningPlayerNumber);
    }

    private PlayerData getCurrentlyMovingPlayer()
    {
        if(CurrentMovingPlayerNumber == PlayerNumber.FirstPlayer) {
            return firstPlayer;
        }
        return secondPlayer;
    }

    private PlayerData getOtherPlayer()
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
