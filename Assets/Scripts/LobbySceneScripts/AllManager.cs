using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using com.example;
using System.Linq;
public class AllManager : NetworkBehaviour
{
    [SerializeField] private SupabaseManager SupabaseManager = null!;
    // Page 1
    [SerializeField] private Button authenticateButton;
    [SerializeField] private Button executeQueriesButton;
    // Page 2
    [SerializeField] private Button CreateLobbyButton;
    [SerializeField] private Button QuickJoinLobbyButton;
    // Page 3
    [SerializeField] private Button StartGameButton;
    [SerializeField] private Button LeaveLobbyButton;
    [SerializeField] private TextMeshProUGUI PlayerListText;
    [SerializeField] private TextMeshProUGUI NumberOfPlayersText;
    // Page 4
    [SerializeField] private Button ShowLeaderBoardButton;
    [SerializeField] private Button ShowPlayerStatsButton;
    [SerializeField] private TextMeshProUGUI Column1QueryAnswer;
    [SerializeField] private TextMeshProUGUI Column2QueryAnswer;
    [SerializeField] private TextMeshProUGUI Column3QueryAnswer;
    [SerializeField] private Button BackToPage1Button;

    // Global
    private string playerName = SignUpAndLogin.playerName;
    private bool IsGaming = false;
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    [SerializeField] private TextMeshProUGUI PlayerNameText;
    private Lobby joinedLobby;
    private float heartBeatTimer;
    private float pollForUpdatesTimer;
    private string NextSceneName = "Scene3";
    public static string hostName;
    public static string joinedLobbyID;


    private void Awake()
    {
        PlayerNameText.text = "";
        authenticateButton.onClick.AddListener(() =>
        {
            AuthenticateWithName(playerName);
        });

        CreateLobbyButton.onClick.AddListener(() =>
        {
            CreateNewLobby();
            SetPage2(false);
        });

        QuickJoinLobbyButton.onClick.AddListener(() =>
        {
            QuickJoinLobby();
        });

        StartGameButton.onClick.AddListener(() =>
        {
            StartGameRelayRpc(hostName);
        });

        LeaveLobbyButton.onClick.AddListener(() =>
        {
            LeaveLobby();
            SetPage2(true);
            SetPage3(false);
        });

        executeQueriesButton.onClick.AddListener(() =>
        {
            SetPage1(false);
            SetPage4(true);
        });

        BackToPage1Button.onClick.AddListener(() =>
        {
            SetPage1(true);
            SetPage4(false);
            Debug.Log("Back to Page 1 Pressed");
        });

        ShowLeaderBoardButton.onClick.AddListener(
            async () => await GetLeaderboard()
        );

        ShowPlayerStatsButton.onClick.AddListener(
            async () => await GetUserStats(playerName)
        );

        SetPage2(false);
        SetPage3(false);
        SetPage4(false);
        StartGameButton.gameObject.SetActive(false);

        DontDestroyOnLoad(gameObject);
    }

    private void SetPage1(bool Pstate)
    {
        authenticateButton.gameObject.SetActive(Pstate);
        executeQueriesButton.gameObject.SetActive(Pstate);
    }

    private void SetPage2(bool Pstate)
    {
        CreateLobbyButton.gameObject.SetActive(Pstate);
        QuickJoinLobbyButton.gameObject.SetActive(Pstate);
    }

    private void SetPage3(bool Pstate)
    {
        if (IsLobbyHost())
        {
            StartGameButton.gameObject.SetActive(Pstate);
        }
        LeaveLobbyButton.gameObject.SetActive(Pstate);
        PlayerListText.gameObject.SetActive(Pstate);
        NumberOfPlayersText.gameObject.SetActive(Pstate);
    }

    private void SetPage4(bool Pstate)
    {

        ShowLeaderBoardButton.gameObject.SetActive(Pstate);
        ShowPlayerStatsButton.gameObject.SetActive(Pstate);
        Column1QueryAnswer.gameObject.SetActive(Pstate);
        Column2QueryAnswer.gameObject.SetActive(Pstate);
        Column3QueryAnswer.gameObject.SetActive(Pstate);
        BackToPage1Button.gameObject.SetActive(Pstate);
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdates();
    }

    private async void AuthenticateWithName(string name)
    {
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(name);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("Player Name: " + playerName);
        PlayerNameText.text = playerName;

        SetPage1(false);

        SetPage2(true);
    }

    private async Task GetLeaderboard()
    {
        var users = await SupabaseManager.Supabase().From<Users>()
        .Select(x => new object[] { x.Name, x.Wins })
        .Limit(10)
        .Order(x => x.Wins, Postgrest.Constants.Ordering.Descending)
        .Get();
        var ListOfUsers = users.Models;
        string LeaderBoardName = "Name\n";
        string LeaderBoardWins = "Wins\n";
        foreach (var user in ListOfUsers)
        {
            // Debug.Log(user.Name + " " + user.Wins);
            LeaderBoardName += user.Name + "\n";
            LeaderBoardWins += user.Wins + "\n";
        }
        SetColumn1AnswerText(LeaderBoardName);
        SetColumn2AnswerText(LeaderBoardWins);
        SetColumn3AnswerText("");
    }
    private async Task GetUserStats(string playerName)
    {
        var users = await SupabaseManager.Supabase().From<Users>()
        .Select(x => new object[] { x.Wins })
        .Where(x => x.Name == playerName)
        .Get();
        var UserData = users.Models;
        string UserWins = UserData[0].Wins.ToString();

        var wordscore = await SupabaseManager.Supabase().From<WordScore>()
        .Select(x => new object[] { x.Score })
        .Where(x => x.Username == playerName)
        .Get();
        var WordScoreData = wordscore.Models;
        List<float> ScoreList = new List<float>();
        foreach (var score in WordScoreData)
        {
            ScoreList.Add(score.Score);
        }
        float averageScore = ScoreList.Average();
        SetColumn1AnswerText("Name\n" + playerName);
        SetColumn2AnswerText("Wins\n" + UserWins);
        SetColumn3AnswerText("Average Score\n" + averageScore.ToString());
    }

    private void SetColumn1AnswerText(string QueriesAnswer)
    {
        Column1QueryAnswer.text = QueriesAnswer;
    }

    private void SetColumn2AnswerText(string QueriesAnswer)
    {
        Column2QueryAnswer.text = QueriesAnswer;
    }

    private void SetColumn3AnswerText(string QueriesAnswer)
    {
        Column3QueryAnswer.text = QueriesAnswer;
    }


    // Lobby Section
    private async void HandleLobbyHeartBeat()
    {
        if (IsLobbyHost())
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer < 0f)
            {
                float heartBeatTimerMax = 15f;
                heartBeatTimer = heartBeatTimerMax;
                Debug.Log("Heartbeat");
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
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
                float pollForUpdatesTimerMax = 2f;
                pollForUpdatesTimer = pollForUpdatesTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
                SetPlayerListText(joinedLobby);
                if (IsLobbyHost() && !IsGaming)
                {
                    StartGameButton.gameObject.SetActive(true);
                }
                if (!IsPlayerInLobby())
                {
                    // Player was kicked out of this lobby
                    Debug.Log("Kicked from Lobby!");
                    joinedLobby = null;
                }
            }
        }
    }

    public async void CreateNewLobby()
    {
        try
        {
            string lobbyName = playerName + "'s Word Game Lobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            joinedLobby = lobby;
            hostName = playerName;
            joinedLobbyID = joinedLobby.Id;

            Allocation allocation = await AllocateRelay();

            string relayJoinCode = await GetRelayJoinCode(allocation);
            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            NetworkManager.Singleton.StartHost();

            Debug.Log("Lobby created: With ID:" + lobby.Id + " with name: " + lobby.Name + " and max players: " + lobby.MaxPlayers);

            PrintPlayersInLobby(joinedLobby);

            SetPage2(false);
            SetPage3(true);
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
            joinedLobbyID = joinedLobby.Id;
            string Relayjoincode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinAllocation = await JoiningTheRelay(Relayjoincode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            NetworkManager.Singleton.StartClient();
            joinedLobbyID = joinedLobby.Id;

            PrintPlayersInLobby(joinedLobby);
            SetPage2(false);
            SetPage3(true);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                joinedLobby = null;
                joinedLobbyID = null;
                hostName = null;
                SetPage2(true);
                SetPage3(false);
                StartGameButton.gameObject.SetActive(false);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }


    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }

    private void SetPlayerListText(Lobby lobby)
    {
        string playerList = "";
        foreach (Player player in lobby.Players)
        {
            playerList += player.Data["PlayerName"].Value + "\n";
        }
        PlayerListText.text = playerList;
        NumberOfPlayersText.text = "Number of Players: " + lobby.Players.Count + "/" + lobby.MaxPlayers;
    }

    public void PrintPlayersInLobby(Lobby lobby)
    {
        Debug.Log("There are " + lobby.Players.Count + " Players in lobby: " + lobby.Name);
        foreach (Player player in lobby.Players)
        {
            Debug.Log("Player ID: " + player.Id + " with name: " + player.Data["PlayerName"].Value);
        }
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

    // Relay Section
    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            return allocation;

        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string RelayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code: " + RelayJoinCode);
            return RelayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
            return default;
        }
    }

    private async Task<JoinAllocation> JoiningTheRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay with joinCode: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("Allocation joined " + joinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
            return default;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void StartGameRelayRpc(string host_Name)
    {
        Debug.Log("Starting the Game Relay");
        SetPage3(false); //not needed as we are starting another scene
        IsGaming = true;
        Debug.Log("IsGaming: " + IsGaming);
        hostName = host_Name;
        if (IsLobbyHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene(NextSceneName, LoadSceneMode.Single);
        }
    }

}
