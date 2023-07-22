using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListUI2 : MonoBehaviour {


    public static LobbyListUI2 Instance { get; private set; }

    [SerializeField] Transform lobbyListContainer;
    [SerializeField] Transform lobbySingleTemplate;
    [SerializeField] Button refreshBtn;
    [SerializeField] Button createLobbyBtn;
    [SerializeField] Button joinWithCodeBtn;


    private void Awake() 
    {
        Instance = this;

        lobbySingleTemplate.gameObject.SetActive(false);

        refreshBtn.onClick.AddListener(RefreshButtonClicked);
        createLobbyBtn.onClick.AddListener(CreateLobbyBtnClicked);
    }

    private void Start() {
        LobbyManager2.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager2.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager2.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager2.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager2.LobbyEventArgs e) {
        Show();
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e) {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager2.LobbyEventArgs e) {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager2.OnLobbyListChangedEventArgs e) {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList) 
    {
        foreach (Transform child in lobbyListContainer) {
            if (child == lobbySingleTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList) 
        {
            Transform lobbyTempleteTransform = Instantiate(lobbySingleTemplate, lobbyListContainer);
            lobbyTempleteTransform.gameObject.SetActive(true);
            LobbyListSingleUI2 lobbyListSingleUI = lobbyTempleteTransform.GetComponent<LobbyListSingleUI2>();
            lobbyListSingleUI.UpdateLobby(lobby);
        }
    }

    private void RefreshButtonClicked() 
    {
        LobbyManager2.Instance.RefreshLobbyList();
    }

    private void CreateLobbyBtnClicked() 
    {
        LobbyCreateUI2.Instance.Show();
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