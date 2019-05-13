using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown firstPlayerTypeDropdown;
    [SerializeField] private TMP_Dropdown firstPlayerAlgorithmDropdown;
    [SerializeField] private TMP_Dropdown firstPlayerHeuristicDropdown;

    [SerializeField] private TMP_Dropdown secondPlayerTypeDropdown;
    [SerializeField] private TMP_Dropdown secondPlayerAlgorithmDropdown;
    [SerializeField] private TMP_Dropdown secondPlayerHeuristicDropdown;

    [SerializeField] private TextMeshProUGUI numberOfMovesText;
    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private string numberOfMovesTemplateText = "Moves: {0}";
    [SerializeField] private string timerTemplateText = "Time[s]: {0}";
    [SerializeField] private string currentMovingPlayerTemplateText = "Turn: Player {0}";
    [SerializeField] private TextMeshProUGUI currentMovingPlayerText;

    [SerializeField] private Button playButton;
    [SerializeField] private Toggle logToFileToggle;

    [SerializeField] private Button[] pawnButtons;

    [SerializeField] private Sprite firstPlayerPawnImage;
    [SerializeField] private Sprite secondPlayerPawnImage;
    [SerializeField] private Sprite emptyField;

    private Color emptyColor = new Color(255, 255, 255, 0);
    private Color nonEmptyColor = new Color(255, 255, 255, 255);

    private Color firstPlayerColor = new Color(255, 255, 255, 255);
    private Color secondPlayerColor = new Color(0, 0, 0, 255);

    private GameEngine gameEngine = null;
    private PlayersController playersController = null;


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
        if(playerNumber == PlayerNumber.SecondPlayer)
        {
            algorithmDropdown = secondPlayerAlgorithmDropdown;
            heuristicDropdown = secondPlayerHeuristicDropdown;
        }
        if (playerType == 0)
        {
            algorithmDropdown.interactable = false;
            heuristicDropdown.interactable = false;
        }
        else
        {
            algorithmDropdown.interactable = true;
            heuristicDropdown.interactable = true;
        }
    }


    void StartGame()
    {
        gameEngine = new GameEngine(false);
        //AiPlayer aiPlayer = new RandomAiPlayer(PlayerNumber.FirstPlayer, gameEngine);
        //AiPlayer aiPlayer2 = new RandomAiPlayer(PlayerNumber.SecondPlayer, gameEngine);
        Heuristic h1 = new SimplePawnNumberHeuristic();
        AiPlayer aiPlayer = new RandomAiPlayer(PlayerNumber.FirstPlayer, gameEngine);
        //AiPlayer aiPlayer = new MinMaxAiPlayer(gameEngine, h1, PlayerNumber.FirstPlayer, 1);
        //AiPlayer aiPlayer = new MinMaxAiPlayer(gameEngine, h1, PlayerNumber.FirstPlayer, 1);
        AiPlayer aiPlayer2 = new AlphaBetaAiPlayer(gameEngine, h1, PlayerNumber.SecondPlayer, 3);
        playersController = new PlayersController(aiPlayer, aiPlayer2);
        //playersController = new PlayersController(secondAiPlayer: aiPlayer2);
        OnBoardUpdated(gameEngine.GameState.CurrentBoard);
        gameEngine.OnBoardChanged += OnBoardUpdated;
        gameEngine.OnGameFinished += OnGameFinished;
        gameEngine.OnPlayerTurnChanged += OnPlayerTurnChanged;
        gameEngine.OnPlayerTurnChanged += playersController.OnPlayerTurnChanged;
        playButton.interactable = false;
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
        Debug.Log(string.Format("Player {0} won!", winningPlayer));
        gameEngine.OnBoardChanged -= OnBoardUpdated;
        gameEngine.OnGameFinished -= OnGameFinished;
        gameEngine.OnPlayerTurnChanged -= OnPlayerTurnChanged;
        gameEngine = null;
    }

    private void HandleButtonClick(int fieldIndex)
    {
        if(gameEngine != null)
        {
            Debug.Log(fieldIndex);
            gameEngine.HandleSelection(fieldIndex);
        }
    }

    private void SetUiActive(bool active)
    {

    }


    private void Update()
    {
        if(playersController != null)
        {
            playersController.CheckStep();
        }
        if (gameEngine != null)
        {
            List<int> possibleMoveIndices = gameEngine.GetCurrentPossibleMoves();
            for(int i = 0; i < pawnButtons.Length; i++)
            {
                Image[] images = pawnButtons[i].GetComponentsInChildren<Image>();
                if (possibleMoveIndices.Contains(i))
                {
                    images[1].enabled = true;
                } else
                {
                    images[1].enabled = false;
                }
            }
        }
    }
}
