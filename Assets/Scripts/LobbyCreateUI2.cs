using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI2 : MonoBehaviour {


    public static LobbyCreateUI2 Instance { get; private set; }

    [SerializeField] Button backBtn;
    [SerializeField] Button createBtn;
    [SerializeField] Button lobbyNameBtn;
    [SerializeField] Button maxPlayersBtn;
    [SerializeField] Button gameModeBtn;
    [SerializeField] Button publicPrivateBtn;
    [SerializeField] TextMeshProUGUI lobbyNameText;
    [SerializeField] TextMeshProUGUI maxPlayersText;
    [SerializeField] TextMeshProUGUI gameModeText;
    [SerializeField] TextMeshProUGUI publicPrivateText;

    string lobbyName;
    bool isPrivate;
    int maxPlayers;
    LobbyManager2.GameMode gameMode;

    private void Awake() {
        Instance = this;

        backBtn.onClick.AddListener(Hide);

        createBtn.onClick.AddListener(() => 
        {
            LobbyManager2.Instance.CreateLobby(lobbyName, maxPlayers, gameMode, isPrivate);
            Hide();
        });

        lobbyNameBtn.onClick.AddListener(() => 
        {
            InputBlocker.Show_Static();
            InputWindow.ShowString_Static("Lobby Name", lobbyName, "abcdefghijklmnopqrstuvxywzABCDEFGHIJKLMNOPQRSTUVXYWZ .,-", 20,
            () => 
            {
                // Cancel
                InputBlocker.Hide_Static();
            },
            (string lobbyName) => 
            {
                // submit
                InputBlocker.Hide_Static();
                this.lobbyName = lobbyName;
                UpdateText();
            });
        });

        publicPrivateBtn.onClick.AddListener(() => 
        {
            isPrivate = !isPrivate;
            UpdateText();
        });

        maxPlayersBtn.onClick.AddListener(() => 
        {
            InputBlocker.Show_Static();
            InputWindow.ShowInt_Static("Max Players", maxPlayers,
            () => 
            {
                // Cancel
                InputBlocker.Hide_Static();
            },
            (int maxPlayers) => 
            {
                // Submit
                InputBlocker.Hide_Static();
                this.maxPlayers = maxPlayers;
                UpdateText();
            });
        });

        gameModeBtn.onClick.AddListener(() => 
        {
            switch (gameMode) 
            {
                default:
                case LobbyManager2.GameMode.DeathMatch:
                    gameMode = LobbyManager2.GameMode.TeamDeathMatch;
                    break;
                case LobbyManager2.GameMode.TeamDeathMatch:
                    gameMode = LobbyManager2.GameMode.DeathMatch;
                    break;
            }
            UpdateText();
        });

        Hide();
    }

    private void UpdateText() 
    {
        Debug.Log(lobbyName);
        Debug.Log(maxPlayers);
        lobbyNameText.text = lobbyName;
        publicPrivateText.text = isPrivate ? "Private" : "Public";
        maxPlayersText.text = maxPlayers.ToString();
        gameModeText.text = gameMode.ToString();
    }

    private void Hide() 
    {
        gameObject.SetActive(false);
    }

    public void Show() 
    {
        gameObject.SetActive(true);

        lobbyName = "My Lobby";
        maxPlayers = 4;
        gameMode = LobbyManager2.GameMode.TeamDeathMatch;
        isPrivate = false;

        UpdateText();
    }

}