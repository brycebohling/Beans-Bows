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

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; } 

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

    public const string KEY_START_GAME_CODE = "0";
    public const string KEY_GAME_MODE = "GameMode";
    public const string KEY_PLAYER_NAME = "PlayerName";

    Lobby joinedLobby;
    float heartBeatTimer;
    float lobbyPollTimer;
    float lobbyListRefreshTimerMax = 3f;
    float lobbyListRefreshTimer = 3;
    string playerName;
    

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
                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                    joinedLobby = null;

                } else if (joinedLobby.Data[KEY_START_GAME_CODE].Value != "0" && !IsLobbyHost())
                {
                    JoinGame();
                }
            }
        } else
        {
            lobbyListRefreshTimer -= Time.deltaTime;
            if (lobbyListRefreshTimer < 0)
            {
                RefreshLobbyList();
                lobbyListRefreshTimer = lobbyListRefreshTimerMax;
            }
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, GameMode gameMode, bool isPrivate) 
    {
        Player player = GetPlayer();

        CreateLobbyOptions options = new CreateLobbyOptions 
        {
            Player = player,
            IsPrivate = isPrivate,
            Data = new Dictionary<string, DataObject> 
            {
                { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) },
                { KEY_START_GAME_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0") }
            }
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        joinedLobby = lobby;

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    public async void JoinLobbyWithCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions 
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            joinedLobby = lobby;

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
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
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

    public async void RefreshLobbyList() 
    {
        if (lobbyListRefreshTimer < 0f)
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
    }

    public Lobby GetJoinedLobby() 
    {
        return joinedLobby;
    }

    public bool IsLobbyHost()
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
                    Debug.Log("Morning");
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

    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await Lobbies.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);

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

                OnLeftLobby?.Invoke(this, EventArgs.Empty);

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
