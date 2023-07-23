using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI2 : MonoBehaviour {


    public static LobbyUI2 Instance { get; private set; }


    [SerializeField] Transform playerSingleTemplate;
    [SerializeField] Transform container;
    [SerializeField] TextMeshProUGUI lobbyNameText;
    [SerializeField] TextMeshProUGUI playerCountText;
    [SerializeField] TextMeshProUGUI gameModeText;
    [SerializeField] TextMeshProUGUI lobbyCodeText;
    [SerializeField] Button leaveLobbyButton;

    float playerListStartY = 215;
    float playerListOffsetY = 125;


    private void Awake() 
    {
        Instance = this;

        playerSingleTemplate.gameObject.SetActive(false);

        leaveLobbyButton.onClick.AddListener(() => 
        {
            LobbyManager2.Instance.LeaveLobby();
        });
    }

    private void Start() 
    {
        LobbyManager2.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager2.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager2.Instance.OnLobbyGameModeChanged += UpdateLobby_Event;
        LobbyManager2.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager2.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;

        Hide();
    }

    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e) 
    {
        ClearLobby();
        Hide();
    }

    private void UpdateLobby_Event(object sender, LobbyManager2.LobbyEventArgs e) 
    {
        UpdateLobby();
    }

    private void UpdateLobby() 
    {
        UpdateLobby(LobbyManager2.Instance.GetJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby) 
    {
        ClearLobby();
        
        int i = 0;
        foreach (Player player in lobby.Players) 
        {   
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.localPosition = new Vector2(0, playerListStartY - playerListOffsetY * i);
            playerSingleTransform.gameObject.SetActive(true);
            LobbyPlayerSingleUI2 lobbyPlayerSingleUI2 = playerSingleTransform.GetComponent<LobbyPlayerSingleUI2>();
            
            lobbyPlayerSingleUI2.SetKickPlayerButtonVisible(
                LobbyManager2.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            lobbyPlayerSingleUI2.UpdatePlayer(player);
            i++;
        }

        lobbyNameText.text = lobby.Name;
        playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        gameModeText.text = lobby.Data[LobbyManager2.KEY_GAME_MODE].Value;
        lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;

        Show();
    }

    private void ClearLobby() 
    {
        foreach (Transform child in container) 
        {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    private void Hide() 
    {
        gameObject.SetActive(false);
    }

    private void Show() 
    {
        gameObject.SetActive(true);
    }

}