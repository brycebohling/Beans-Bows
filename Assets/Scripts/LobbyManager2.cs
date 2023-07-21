using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;

public class LobbyManager2 : MonoBehaviour
{
    public static event Action onKickedFromLobby;

    [SerializeField] Button createLobbyBtn;
    [SerializeField] Button listLobbiesBtn;
    [SerializeField] Button joinLobbyBtn;
    [SerializeField] TMP_InputField joinCodeInput;
    [SerializeField] Button leaveLobbyBtn;
    [SerializeField] Button printPlayersBtn;
    [SerializeField] Button startGameBtn;
    Lobby hostLobby;
    Lobby joinedLobby;
    float heartBeatTimer;
    float lobbyPollTimer;
    string playerName;
    string KEY_START_GAME_CODE = "0";
    string KEY_GAME_MODE = "GameMode";


    private void Awake() 
    {
        // 
        // Might need to to event listener remove and add on enable and disable
        // 
        createLobbyBtn.onClick.AddListener(() => 
        {
            CreateLobby();
        });

        listLobbiesBtn.onClick.AddListener(() => 
        {
            ListLobbies();
        });

        joinLobbyBtn.onClick.AddListener(() => 
        {
            JoinLobbyByCode(joinCodeInput.text);
        });

        leaveLobbyBtn.onClick.AddListener(() => 
        {
            LeaveLobby();
        });

        printPlayersBtn.onClick.AddListener(() => 
        {
            PrintLobby();
        });

        startGameBtn.onClick.AddListener(() => 
        {
            StartGame();
        });
    }

    private async void Start() 
    {
        onKickedFromLobby?.Invoke();
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.ClearSessionToken();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Sign in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Bryce" + UnityEngine.Random.Range(1, 100);
        
        Debug.Log(playerName);
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPolling();
    }

    private async void HandleLobbyHeartBeat()
    {
        if (IsLobbyHost())
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer < 0f)
            {
                float heartBeatTimerMax = 15f;
                heartBeatTimer = heartBeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPolling() 
    {
        if (joinedLobby != null)
        {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                float lobbyPollTimerMax = 1.1f;
                lobbyPollTimer = lobbyPollTimerMax;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                
                if (!IsPlayerStillInLobby())
                {
                    Debug.Log("Kicked from Lobby!");

                    joinedLobby = null;
                }

                if (joinedLobby.Data[KEY_START_GAME_CODE].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        RelayManager.Instance.JoinRelay(joinedLobby.Data[KEY_START_GAME_CODE].Value);
                    }

                    joinedLobby = null;
                }
            }
        }
    }

    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject> {
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, "TeamDeathMatch")},
                    { KEY_START_GAME_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0")},
                }
            };

            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;

            Debug.Log("Created Lobby: " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);

            PrintPlayers(hostLobby);

        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions {
                Count = 25,
                Filters = new List<QueryFilter> {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder> {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby _lobby in queryResponse.Results)
            {
                Debug.Log(_lobby.Name + " " + _lobby.MaxPlayers + " " + _lobby.Data[KEY_GAME_MODE].Value);
            }

        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            joinedLobby = lobby;

            Debug.Log("Joined lobby with code: " + lobbyCode);

            PrintPlayers(joinedLobby);

        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }        
    }

    private Player GetPlayer()
    {
        return new Player {
            Data = new Dictionary<string, PlayerDataObject> {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
            }
        };
    } 

    private void PrintLobby()
    {
        PrintPlayers(joinedLobby);
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in lobby " + lobby.Name + lobby.Data[KEY_GAME_MODE].Value);
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
                }
            });

            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);

        } catch (LobbyServiceException e) 
        {
            Debug.Log(e);
        }
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerStillInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null) 
        {
            foreach (Player player in joinedLobby.Players) 
            {
                if (player.Id == AuthenticationService.Instance.PlayerId) 
                {
                    return true;
                }
            }
        }

        return false;
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await Lobbies.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions {
                Data = new Dictionary<string, PlayerDataObject> {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                }
            });
        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void KickPlayer(string playerID)
    {
        if (IsLobbyHost())
        {
            try
            {
                await Lobbies.Instance.RemovePlayerAsync(joinedLobby.Id, playerID);

                Debug.Log("Left lobby");
            } catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
        
    }

    public async void LeaveLobby() 
    {
        if (joinedLobby != null) 
        {
            try 
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;

            } catch (LobbyServiceException e) 
            {
                Debug.Log(e);
            }
        }
    }

    private async void StartGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("Starting Game");

                string relayCode = await RelayManager.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                    Data = new Dictionary<string, DataObject> {
                        { KEY_START_GAME_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });
            } catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}
