
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;
using UnityEngine.UI;

public class WordGameLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartBeatTimer;
    private float pollForUpdatesTimer;
    private string playerName;
    public TMP_InputField codeInputField;
    [SerializeField] private Button _CreateLobbyButton;
    [SerializeField] private Button _CreatePrivateLobbyButton;
    [SerializeField] private Button _QuickJoinLobbyButton;
    [SerializeField] private Button _JoinLobbyButton;
    [SerializeField] private Button _UpdateLobbyGameModeButton;
    [SerializeField] private Button _LeaveLobbyButton;
    [SerializeField] private Button _KickPlayerButton;

    private void Awake()
    {
        _CreateLobbyButton.onClick.AddListener(() =>
        {
            Debug.Log("Creating lobby");
            CreateLobby();
        });

        _QuickJoinLobbyButton.onClick.AddListener(() =>
        {
            QuickJoinLobby();
        });

        _JoinLobbyButton.onClick.AddListener(() =>
        {
            Debug.Log("Joining lobby by code: " + codeInputField.text);
            JoinLobbyByCode(codeInputField.text);
        });

        _CreatePrivateLobbyButton.onClick.AddListener(() =>
        {
            Debug.Log("Creating private lobby");
            CreatePrivateLobby();
        });

        _UpdateLobbyGameModeButton.onClick.AddListener(() =>
        {
            UpdateLobbyGameMode("3v3");
        });

        _LeaveLobbyButton.onClick.AddListener(() =>
        {
            LeaveLobby();
        });

        _KickPlayerButton.onClick.AddListener(() =>
        {
            KickPlayer();
        });

    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private async void HandleLobbyHeartBeat()
    {
        if (hostLobby != null)
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

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            pollForUpdatesTimer -= Time.deltaTime;
            if (pollForUpdatesTimer < 0f)
            {
                float pollForUpdatesTimerMax = 1.5f;
                pollForUpdatesTimer = pollForUpdatesTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }   

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Player " + Random.Range(10, 99);
        Debug.Log("Player Name: " + playerName);
    }
    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "WordGameLobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "1v1") }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            joinedLobby = hostLobby;

            Debug.Log("Lobby created: With ID:" + lobby.Id + " with name: " + lobby.Name + " and max players: " + lobby.MaxPlayers);

            PrintPlayersInLobby(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void CreatePrivateLobby()
    {
        try
        {
            string lobbyName = "PrivateWordGameLobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "2v2") }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            joinedLobby = hostLobby;

            Debug.Log("Lobby created: With ID:" + lobby.Id + " with name: " + lobby.Name + " max players: " 
            + lobby.MaxPlayers + " Lobby Code: " + lobby.LobbyCode);
            PrintPlayersInLobby(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0" , QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created) //False means decending order
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log("Lobby ID: " + lobby.Id + " Game Mode: " + lobby.Data["GameMode"].Value +
                " with name: " + lobby.Name + " and max players: " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    // public async void JoinLobby()
    // {
    //     try
    //     {
    //         QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

    //         await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
    //     }
    //     catch (LobbyServiceException e)
    //     {
    //         Debug.Log(e);
    //     }
    // }

    public async void JoinLobbyByCode(string LobbyCode)
    {
        try
        {   JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Debug.Log("Inside Function: Joining lobby with code: " + LobbyCode);
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(LobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            Debug.Log("Joined lobby with code: " + LobbyCode);
            PrintPlayersInLobby(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyByCodeOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyByCodeOptions);
            Debug.Log("Quick join lobby");
            joinedLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void PrintPlayersInLobby(Lobby lobby)
    {
        Debug.Log("There are "+ lobby.Players.Count+ " Players in lobby: " + lobby.Name + " Game Mode: " + lobby.Data["GameMode"].Value);
        foreach (Player player in lobby.Players)
        {
            Debug.Log("Player ID: " + player.Id + " with name: " + player.Data["PlayerName"].Value);
        }
    }

    private void PrintPlayers()
    {
        PrintPlayersInLobby(joinedLobby);
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            //need to assign the updated lobby to hostLobby as it does not automatically update
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                }
            });
            joinedLobby = hostLobby;
            PrintPlayersInLobby(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id); //kick the second player
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdates();
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreateLobby();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ListLobbies();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintPlayers();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            UpdatePlayerName("NewPlayerName1");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            DeleteLobby();
        }

    }


}
