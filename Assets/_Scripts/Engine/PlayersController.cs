using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class PlayersController
{
    private AiPlayer firstAiPlayer;
    private AiPlayer secondAiPlayer;
    private bool gameEngineReady;
    private PlayerNumber currentPlayerTurn;

    public PlayersController(AiPlayer firstAiPlayer = null, AiPlayer secondAiPlayer = null)
    {
        this.firstAiPlayer = firstAiPlayer;
        this.secondAiPlayer = secondAiPlayer;
        this.currentPlayerTurn = PlayerNumber.FirstPlayer;
        this.gameEngineReady = true;
    }

    public void CheckStep()
    {
        if (gameEngineReady)
        {
            if(currentPlayerTurn == PlayerNumber.FirstPlayer)
            {
                HandleAiMove(firstAiPlayer);
            }
            else
            {
                HandleAiMove(secondAiPlayer);
            }
        }
    }

    public void OnPlayerTurnChanged(PlayerNumber playerNumber)
    {
        this.currentPlayerTurn = playerNumber;
        this.gameEngineReady = true;
    }

    private void HandleAiMove(AiPlayer player)
    {
        gameEngineReady = false;
        if(player != null)
        {
            player.MakeMove();
        }
    }
}
