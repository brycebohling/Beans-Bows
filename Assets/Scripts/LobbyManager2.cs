using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class LobbyManager2 : MonoBehaviour
{
    public static LobbyManager2 Instance { get; private set; } 

    public event EventHandler OnLeftLobby;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<LobbyEventArgs> OnLobbyGameModeChanged;
    public class LobbyEventArgs : EventArgs 
    {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs 
    {
        public List<Lobby> lobbyList;
    }

    public enum GameMode 
    {
        DeathMatch,
        TeamDeathMatch
    } 

    Lobby joinedLobby;
    float heartBeatTimer;
    float lobbyPollTimer;
    string playerName;
    const string KEY_START_GAME_CODE = "0";
    const string KEY_GAME_MODE = "GameMode";


    private void Awake() 
    {
        Instance = this;
    }

    private async void Start() 
    {
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

                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
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

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                
                if (!IsPlayerStillInLobby())
                {
                    Debug.Log("Kicked from Lobby!");

                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                    joinedLobby = null;
                }

                if (joinedLobby.Data[KEY_START_GAME_CODE].Value != "0" && !IsLobbyHost())
                {
                    JoinGame();
                }
            }
        }
    }

    public async void CreateLobby()
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

            joinedLobby = lobby;

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

            Debug.Log("Joined Lobby: " + joinedLobby.Name);

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
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions 
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            joinedLobby = lobby;

            Debug.Log("Joined lobby with code: " + lobbyCode);

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }        
    }

    public async void JoinLobby(Lobby lobby) 
    {
        Player player = GetPlayer();

        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions 
        {
            Player = player
        });

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    private Player GetPlayer()
    {
        return new Player {
            Data = new Dictionary<string, PlayerDataObject> 
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
            }
        };
    } 

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in lobby " + lobby.Name + lobby.Data[KEY_GAME_MODE].Value);
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    public async void UpdateLobbyGameMode(GameMode gameMode) 
    {
        try 
        {
            Debug.Log("UpdateLobbyGameMode " + gameMode);
            
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> 
                {
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) }
                }
            });

            joinedLobby = lobby;

            OnLobbyGameModeChanged?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

        } catch (LobbyServiceException e) 
        {
            Debug.Log(e);
        }
    }

    public async void RefreshLobbyList() 
    {
        try 
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter> 
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> 
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });

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
                SceneManager.LoadScene("Arena");

                string relayCode = await RelayManager.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                    Data = new Dictionary<string, DataObject> {
                        { KEY_START_GAME_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                joinedLobby = null;

            } catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    private void JoinGame()
    {
        RelayManager.Instance.JoinRelay(joinedLobby.Data[KEY_START_GAME_CODE].Value);

        joinedLobby = null;
    }
}
