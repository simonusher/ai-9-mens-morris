using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersController
{
    private AiPlayer firstAiPlayer;
    private AiPlayer secondAiPlayer;

    public PlayersController(AiPlayer firstAiPlayer = null, AiPlayer secondAiPlayer = null)
    {
        this.firstAiPlayer = firstAiPlayer;
        this.secondAiPlayer = secondAiPlayer;
    }

    public void StartGame()
    {
        HandleAiMove(firstAiPlayer);
    }

    public void OnPlayerTurnChanged(PlayerNumber playerNumber)
    {
        if(playerNumber == PlayerNumber.FirstPlayer)
        {
            HandleAiMove(firstAiPlayer);
        } else
        {
            HandleAiMove(secondAiPlayer);
        }
    }

    private void HandleAiMove(AiPlayer player)
    {
        if(player != null)
        {
            player.MakeMove();
        }
    }
}
