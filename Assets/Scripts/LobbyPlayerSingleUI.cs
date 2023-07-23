using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class LobbyPlayerSingleUI : MonoBehaviour {


    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] Button kickPlayerButton;

    Player player;


    private void Awake() 
    {
        kickPlayerButton.onClick.AddListener(KickPlayer);
    }

    public void SetKickPlayerButtonVisible(bool visible) 
    {
        kickPlayerButton.gameObject.SetActive(visible);
    }

    public void UpdatePlayer(Player player) {
        this.player = player;
        playerNameText.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
    }

    private void KickPlayer() {
        if (player != null) 
        {
            LobbyManager.Instance.KickPlayer(player.Id);
        }
    }


}