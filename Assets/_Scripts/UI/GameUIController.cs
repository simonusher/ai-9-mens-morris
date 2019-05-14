using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameUIController : MonoBehaviour
{
    private static readonly int HUMAN_DROPDOWN_NUMBER = 0;
    private static readonly int AI_DROPDOWN_NUMBER = 1;
    private static readonly int MIN_MAX_DROPDOWN_NUMBER = 0;
    private static readonly int ALPHA_BETA_DROPDOWN_NUMBER = 1;

    private static Dictionary<int, Func<Heuristic>> heuristicDictionary;

    [SerializeField] private TMP_Dropdown firstPlayerTypeDropdown = null;
    [SerializeField] private TMP_Dropdown firstPlayerAlgorithmDropdown = null;
    [SerializeField] private TMP_Dropdown firstPlayerHeuristicDropdown = null;
    [SerializeField] private TMP_Dropdown firstPlayerSearchDepthDropdown = null;

    [SerializeField] private TMP_Dropdown secondPlayerTypeDropdown = null;
    [SerializeField] private TMP_Dropdown secondPlayerAlgorithmDropdown = null;
    [SerializeField] private TMP_Dropdown secondPlayerHeuristicDropdown = null;
    [SerializeField] private TMP_Dropdown secondPlayerSearchDepthDropdown = null;

    [SerializeField] private TextMeshProUGUI numberOfMovesText = null;
    [SerializeField] private TextMeshProUGUI timerText = null;

    [SerializeField] private string numberOfMovesTemplateText = "Moves: {0}";
    [SerializeField] private string timerTemplateText = "Time[s]: {0}";
    [SerializeField] private string currentMovingPlayerTemplateText = "Turn: Player {0}";
    [SerializeField] private TextMeshProUGUI currentMovingPlayerText = null;

    [SerializeField] private Button playButton = null;
    [SerializeField] private Toggle logToFileToggle = null;

    [SerializeField] private Button[] pawnButtons = null;

    [SerializeField] private Sprite firstPlayerPawnImage = null;
    [SerializeField] private Sprite secondPlayerPawnImage = null;
    [SerializeField] private Sprite emptyField = null;
    [SerializeField] private string winningPlayerTextTemplate = "Won: ";
    [SerializeField] private TextMeshProUGUI winningPlayerTextField = null;

    private Color emptyColor = new Color(255, 255, 255, 0);
    private Color nonEmptyColor = new Color(255, 255, 255, 255);

    private Color firstPlayerColor = new Color(255, 255, 255, 255);
    private Color secondPlayerColor = new Color(0, 0, 0, 255);

    private GameEngine gameEngine = null;
    private PlayersController aiPlayersController = null;

    private float timePassed;

    static GameUIController()
    {
        heuristicDictionary = new Dictionary<int, Func<Heuristic>>();
        heuristicDictionary[0] = () => new SimplePawnNumberHeuristic();
        heuristicDictionary[1] = () => new PawnMillNumberHeuristic();
        heuristicDictionary[2] = () => new PawnMoveNumberHeuristic();
    }

    private void Awake()
    {
        firstPlayerTypeDropdown.onValueChanged.AddListener(gameType => SetAIDropdownsActive(gameType, PlayerNumber.FirstPlayer));
        secondPlayerTypeDropdown.onValueChanged.AddListener(gameType => SetAIDropdownsActive(gameType, PlayerNumber.SecondPlayer));
        InitPawnButtonHandlers();
        playButton.onClick.AddListener(StartGame);
    }

    private void InitPawnButtonHandlers()
    {
        for(int i = 0; i < pawnButtons.Length; i++)
        {
            int x = i;
            pawnButtons[i].onClick.AddListener(() => HandleButtonClick(x));
        }
    }

    private void SetAIDropdownsActive(int playerType, PlayerNumber playerNumber)
    {
        TMP_Dropdown algorithmDropdown = firstPlayerAlgorithmDropdown;
        TMP_Dropdown heuristicDropdown = firstPlayerHeuristicDropdown;
        TMP_Dropdown searchDepthDropdown = firstPlayerSearchDepthDropdown;
        if(playerNumber == PlayerNumber.SecondPlayer)
        {
            algorithmDropdown = secondPlayerAlgorithmDropdown;
            heuristicDropdown = secondPlayerHeuristicDropdown;
            searchDepthDropdown = secondPlayerSearchDepthDropdown;
        }
        if (playerType == HUMAN_DROPDOWN_NUMBER)
        {
            algorithmDropdown.interactable = false;
            heuristicDropdown.interactable = false;
            searchDepthDropdown.interactable = false;
        }
        else
        {
            algorithmDropdown.interactable = true;
            heuristicDropdown.interactable = true;
            searchDepthDropdown.interactable = true;
        }
    }


    void StartGame()
    {
        gameEngine = new GameEngine(false);
        AiPlayer firstPlayer = InitPlayer(PlayerNumber.FirstPlayer);
        AiPlayer secondPlayer = InitPlayer(PlayerNumber.SecondPlayer);
        aiPlayersController = new PlayersController(firstPlayer, secondPlayer);
        timePassed = 0;
        OnBoardUpdated(gameEngine.GameState.CurrentBoard);
        gameEngine.OnBoardChanged += OnBoardUpdated;
        gameEngine.OnGameFinished += OnGameFinished;
        gameEngine.OnPlayerTurnChanged += OnPlayerTurnChanged;
        gameEngine.OnPlayerTurnChanged += aiPlayersController.OnPlayerTurnChanged;
        playButton.interactable = false;
    }

    private AiPlayer InitPlayer(PlayerNumber playerNumber)
    {
        TMP_Dropdown playerDropdown;
        TMP_Dropdown algorithmDropdown;
        TMP_Dropdown heuristicDropdown;
        TMP_Dropdown searchDepthDropdown;
        if (playerNumber == PlayerNumber.FirstPlayer)
        {
            playerDropdown = firstPlayerTypeDropdown;
            algorithmDropdown = firstPlayerAlgorithmDropdown;
            heuristicDropdown = firstPlayerHeuristicDropdown;
            searchDepthDropdown = firstPlayerSearchDepthDropdown;
        } else
        {
            playerDropdown = secondPlayerTypeDropdown;
            algorithmDropdown = secondPlayerAlgorithmDropdown;
            heuristicDropdown = secondPlayerHeuristicDropdown;
            searchDepthDropdown = secondPlayerSearchDepthDropdown;
        }
        if (playerDropdown.value == AI_DROPDOWN_NUMBER)
        {
            Heuristic heuristic = heuristicDictionary[heuristicDropdown.value]();
            int searchDepth = searchDepthDropdown.value + 1;
            if(algorithmDropdown.value == MIN_MAX_DROPDOWN_NUMBER)
            {
                return new MinMaxAiPlayer(gameEngine, heuristic, playerNumber, searchDepth);
            } else
            {
                return new AlphaBetaAiPlayer(gameEngine, heuristic, playerNumber, searchDepth);
            }
        }
        return null;
    }

    private void OnBoardUpdated(Board newBoard)
    {
        for(int i = 0; i < pawnButtons.Length; i++)
        {
            Field field = newBoard.GetField(i);
            if(field.PawnPlayerNumber == PlayerNumber.FirstPlayer)
            {
                pawnButtons[i].image.sprite = firstPlayerPawnImage;
                pawnButtons[i].image.color = nonEmptyColor;
            } else if (field.PawnPlayerNumber == PlayerNumber.SecondPlayer)
            {
                pawnButtons[i].image.sprite = secondPlayerPawnImage;
                pawnButtons[i].image.color = nonEmptyColor;
            } else
            {
                pawnButtons[i].image.color = emptyColor;
            }
        }
    }

    private void OnPlayerTurnChanged(PlayerNumber currentMovingPlayerNumber)
    {
        if(currentMovingPlayerNumber == PlayerNumber.FirstPlayer)
        {
            UpdateTurnText(1);
        } else
        {
            UpdateTurnText(2);
        }
    }

    private void UpdateTurnText(int playerNumber)
    {
        currentMovingPlayerText.text = string.Format(currentMovingPlayerTemplateText, playerNumber);
        currentMovingPlayerText.faceColor = playerNumber == 1 ? firstPlayerColor : secondPlayerColor;
    }

    private void OnGameFinished(PlayerNumber winningPlayer)
    {
        UpdateWinningPlayerText(winningPlayer);
        gameEngine.OnBoardChanged -= OnBoardUpdated;
        gameEngine.OnGameFinished -= OnGameFinished;
        gameEngine.OnPlayerTurnChanged -= OnPlayerTurnChanged;
        gameEngine.OnPlayerTurnChanged -= aiPlayersController.OnPlayerTurnChanged;
        gameEngine = null;
        aiPlayersController = null;
        playButton.interactable = true;
    }

    private void UpdateWinningPlayerText(PlayerNumber winningPlayer)
    {
        Color winningPlayerColor = firstPlayerColor;
        string winnningPlayerString = "Player 1";
        if(winningPlayer == PlayerNumber.SecondPlayer)
        {
            winnningPlayerString = "Player 2";
            winningPlayerColor = secondPlayerColor;
        }
        winningPlayerTextField.text = winningPlayerTextTemplate + winnningPlayerString;
        winningPlayerTextField.faceColor = winningPlayerColor;
    }

    private void HandleButtonClick(int fieldIndex)
    {
        if(gameEngine != null)
        {
            gameEngine.HandleSelection(fieldIndex);
        }
    }

    private void Update()
    {
        MakeAiControllerStep();
        UpdateGameStateData();
    }

    private void MakeAiControllerStep()
    {
        if (aiPlayersController != null)
        {
            aiPlayersController.CheckStep();
        }
    }

    private void UpdateGameStateData()
    {
        if (gameEngine != null)
        {
            UpdatePossibleMoveIndicators();
            UpdateMoveNumberText();
            UpdateTime();
        }
    }

    private void UpdateTime()
    {
        if (!gameEngine.GameState.GameFinished)
        {
            timePassed += Time.deltaTime;
            timerText.text = string.Format(timerTemplateText, Math.Truncate(timePassed * 100) / 100);
        }
    }

    private void UpdateMoveNumberText()
    {
        numberOfMovesText.text = string.Format(numberOfMovesTemplateText, gameEngine.GameState.MovesMade);
    }

    private void UpdatePossibleMoveIndicators()
    {
        List<int> possibleMoveIndices = gameEngine.GetCurrentPossibleMoves();
        for (int i = 0; i < pawnButtons.Length; i++)
        {
            Image[] images = pawnButtons[i].GetComponentsInChildren<Image>();
            if (possibleMoveIndices.Contains(i))
            {
                images[1].enabled = true;
            }
            else
            {
                images[1].enabled = false;
            }
        }
    }
}
