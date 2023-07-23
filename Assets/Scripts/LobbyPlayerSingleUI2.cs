using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class LobbyPlayerSingleUI2 : MonoBehaviour {


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
        playerNameText.text = player.Data[LobbyManager2.KEY_PLAYER_NAME].Value;
    }

    private void KickPlayer() {
        if (player != null) 
        {
            LobbyManager2.Instance.KickPlayer(player.Id);
        }
    }


}