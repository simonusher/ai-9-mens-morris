﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine
{
    public delegate void GameFinished(PlayerNumber winningPlayerNumber);
    public event GameFinished OnGameFinished = delegate { };

    public delegate void BoardChanged(Board newBoard);
    public event BoardChanged OnBoardChanged = delegate { };

    public delegate void PlayerTurnChanged(PlayerNumber currentMovingPlayerNumber);
    public event PlayerTurnChanged OnPlayerTurnChanged = delegate { };

    private bool shouldLogToFile;

    public GameState GameState { get; private set; }

    private PlayerNumber lastPlayerTurn;


    public GameEngine(bool shouldLogToFile)
    {
        this.shouldLogToFile = shouldLogToFile;
        lastPlayerTurn = PlayerNumber.FirstPlayer;
        RegisterNewGameState(new GameState());
    }

    public void HandleSelection(int selectedFieldIndex)
    {
        GameState.HandleSelection(selectedFieldIndex);
    }

    public void MakeMove(GameState gameState)
    {
        RegisterNewGameState(gameState);
    }

    private void OnGameStateChanged()
    {
        Debug.Log(GameState.WinningPlayer);
        foreach(Mill mill in GameState.ClosedMills)
        {
            Debug.Log(mill);
        }
        OnBoardChanged(GameState.CurrentBoard);
        if (GameState.WinningPlayer != PlayerNumber.None)
        {
            OnGameFinished(GameState.WinningPlayer);
        }
        if (GameState.CurrentMovingPlayer != lastPlayerTurn)
        {
            lastPlayerTurn = GameState.CurrentMovingPlayer;
            OnPlayerTurnChanged(lastPlayerTurn);
        }
    }

    private void RegisterNewGameState(GameState gameState)
    {
        if(GameState != null)
        {
            GameState.OnGameStateChanged -= OnGameStateChanged;
        }
        GameState = gameState;
        GameState.OnGameStateChanged += OnGameStateChanged;
        OnGameStateChanged();
    }

    public List<int> GetCurrentPossibleMoves()
    {
        return GameState.GetCurrentPlayerPossibleMoveIndices();
    }
}
