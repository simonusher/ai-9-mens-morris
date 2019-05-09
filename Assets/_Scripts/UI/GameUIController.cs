using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [SerializeField] private Button playButton;
    [SerializeField] private Toggle logToFileToggle;

    private void Awake()
    {
        firstPlayerTypeDropdown.onValueChanged.AddListener((int type) => SetAIDropdownsActive(PlayerNumber.First, type));
        secondPlayerTypeDropdown.onValueChanged.AddListener((int type) => SetAIDropdownsActive(PlayerNumber.Second, type));
        playButton.onClick.AddListener(StartGame);
    }

    private void SetAIDropdownsActive(PlayerNumber player, int playerType)
    {
        bool dropdownsActive = playerType == (int)PlayerType.AI;
        TMP_Dropdown algorithmDropdown = secondPlayerAlgorithmDropdown;
        TMP_Dropdown heuristicDropdown = secondPlayerHeuristicDropdown;
        if (player == PlayerNumber.First)
        {
            algorithmDropdown = firstPlayerAlgorithmDropdown;
            heuristicDropdown = firstPlayerHeuristicDropdown;
        }

        algorithmDropdown.interactable = dropdownsActive;
        heuristicDropdown.interactable = dropdownsActive;
    }


    void StartGame()
    {
        Debug.Log("Game starting");
    }
}
