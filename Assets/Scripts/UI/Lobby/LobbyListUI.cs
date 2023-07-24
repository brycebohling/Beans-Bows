using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyListUI : MonoBehaviour {


    public static LobbyListUI Instance { get; private set; }

    [SerializeField] Transform lobbyListContainer;
    [SerializeField] Transform lobbySingleTemplate;
    [SerializeField] Button createLobbyBtn;
    [SerializeField] Button joinWithCodeBtn;
    [SerializeField] TMP_InputField joinCodeInput;

    float lobbyListStartY = 220;
    float lobbyListOffsetY = 120;


    private void Awake() 
    {
        Instance = this;

        lobbySingleTemplate.gameObject.SetActive(false);

        createLobbyBtn.onClick.AddListener(CreateLobbyBtnClicked);
        joinWithCodeBtn.onClick.AddListener(() => 
        {
            LobbyManager.Instance.JoinLobbyWithCode(joinCodeInput.text);
        });
    }

    private void Start() {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e) {
        Show();
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e) {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e) {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e) {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList) 
    {
        foreach (Transform child in lobbyListContainer) {
            if (child == lobbySingleTemplate) continue;

            Destroy(child.gameObject);
        }

        int i = 0;
        foreach (Lobby lobby in lobbyList) 
        {
            Transform lobbyTempleteTransform = Instantiate(lobbySingleTemplate, lobbyListContainer);
            lobbyTempleteTransform.localPosition = new Vector2(0, lobbyListStartY - lobbyListOffsetY * i);
            lobbyTempleteTransform.gameObject.SetActive(true);
            LobbyListSingleUI lobbyListSingleUI = lobbyTempleteTransform.GetComponent<LobbyListSingleUI>();
            lobbyListSingleUI.UpdateLobby(lobby);
            i++;
        }
    }

    private void CreateLobbyBtnClicked() 
    {
        LobbyCreateUI.Instance.Show();
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