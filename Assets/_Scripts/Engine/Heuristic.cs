using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Heuristic
{
    double Rate(GameState gameState, PlayerNumber playerNumber);
}
