using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown gameTypeDropdown;
    [SerializeField] private TMP_Dropdown firstPlayerAlgorithmDropdown;
    [SerializeField] private TMP_Dropdown firstPlayerHeuristicDropdown;
    [SerializeField] private TMP_Dropdown secondPlayerAlgorithmDropdown;
    [SerializeField] private TMP_Dropdown secondPlayerHeuristicDropdown;

    [SerializeField] private TextMeshProUGUI numberOfMovesText;
    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private string numberOfMovesTemplateText = "Moves: {0}";
    [SerializeField] private string timerTemplateText = "Time[s]: {0}";

    [SerializeField] private Button playButton;
    [SerializeField] private Toggle logToFileToggle;

    [SerializeField] private Button[] pawnButtons;

    [SerializeField] private Sprite firstPlayerPawnImage;
    [SerializeField] private Sprite secondPlayerPawnImage;

    private GameEngine gameEngine = null;

    private void Awake()
    {
        gameTypeDropdown.onValueChanged.AddListener(gameType => SetAIDropdownsActive(gameType));
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

    private void SetAIDropdownsActive(int gameType)
    {
        if(gameType == 0)
        {
            firstPlayerAlgorithmDropdown.interactable = false;
            firstPlayerHeuristicDropdown.interactable = false;
        }
        else
        {
            firstPlayerAlgorithmDropdown.interactable = true;
            firstPlayerHeuristicDropdown.interactable = true;
        }
    }


    void StartGame()
    {
        gameEngine = new GameEngine(false);
        OnBoardUpdated(gameEngine.currentBoard);
        gameEngine.OnBoardChanged += OnBoardUpdated;
    }

    void OnBoardUpdated(Board newBoard)
    {
        for(int i = 0; i < pawnButtons.Length; i++)
        {
            Field field = newBoard.GetField(i);
            if(field.PawnPlayerNumber == PlayerNumber.FirstPlayer)
            {
                pawnButtons[i].image.sprite = firstPlayerPawnImage;
            } else if (field.PawnPlayerNumber == PlayerNumber.SecondPlayer)
            {
                pawnButtons[i].image.sprite = secondPlayerPawnImage;
            } else
            {
                pawnButtons[i].image.sprite = null;
            }
        }
    }

    void HandleButtonClick(int fieldIndex)
    {
        if(gameEngine != null)
        {
            Debug.Log(fieldIndex);
            gameEngine.HandleSelection(fieldIndex);
        }
    }
}
