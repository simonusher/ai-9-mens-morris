using System.Diagnostics;
public class PlayersController
{
    public AiPlayer firstAiPlayer;
    public AiPlayer secondAiPlayer;
    private bool gameEngineReady;
    private PlayerNumber currentPlayerTurn;
    private Stopwatch stopWatch;

    public long firstPlayerDecisionTimeMillis = 0;
    public long secondPlayerDecisionTimeMillis = 0;

    public PlayersController(AiPlayer firstAiPlayer = null, AiPlayer secondAiPlayer = null)
    {
        this.firstAiPlayer = firstAiPlayer;
        this.secondAiPlayer = secondAiPlayer;
        this.currentPlayerTurn = PlayerNumber.FirstPlayer;
        this.gameEngineReady = true;
    }

    public long CheckStep()
    {
        if (gameEngineReady)
        {
            if(currentPlayerTurn == PlayerNumber.FirstPlayer)
            {
                long time = HandleAiMove(firstAiPlayer);
                firstPlayerDecisionTimeMillis += time;
                return time;
            }
            else
            {
                long time = HandleAiMove(secondAiPlayer);
                secondPlayerDecisionTimeMillis += time;
                return time;
            }
        } else
        {
            return 0;
        }
    }

    public void OnPlayerTurnChanged(PlayerNumber playerNumber)
    {
        this.currentPlayerTurn = playerNumber;
        this.gameEngineReady = true;
    }

    private long HandleAiMove(AiPlayer player)
    {
        gameEngineReady = false;
        if(player != null)
        {
            stopWatch = Stopwatch.StartNew();
            player.MakeMove();
            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }
        return 0;
    }
}
