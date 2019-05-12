using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePawnNumberHeuristic : Heuristic
{
    private static readonly int DEFAULT_WINNING_WEIGHT = 100;

    public double Evaluate(GameState gameState)
    {
        PlayerNumber winningPlayer = gameState.WinningPlayer;
        double evaluation = gameState.FirstPlayersPawnsLeft - gameState.SecondPlayersPawnsLeft;
        if (winningPlayer == PlayerNumber.FirstPlayer)
        {
            evaluation += DEFAULT_WINNING_WEIGHT;
        }
        else if (winningPlayer == PlayerNumber.SecondPlayer)
        {
            evaluation -= DEFAULT_WINNING_WEIGHT;
        }
        return evaluation;
    }
}
