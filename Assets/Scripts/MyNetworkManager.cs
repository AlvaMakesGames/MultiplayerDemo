using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

public class MyNetworkManager : NetworkRoomManager
{
    public List<FirstPersonPlayer> players = new List<FirstPersonPlayer>();
    public List<FirstPersonPlayer> finalScores = new List<FirstPersonPlayer>();
    public List<RoomPlayer> roomPlayers = new List<RoomPlayer>();

    [SerializeField] private Sprite[] teamImages;
    [SerializeField] private LobbyUI lobbyUI = null;
    [SerializeField] private Menu menu = null;
    public Sprite[] TeamImages => teamImages;
    public bool RoundEnded { get; set; }

    public bool AttemptConnectServer { get; set; }

    public string Winner { get; private set; }
    public int NumTiedPlayers { get; private set; }

    
    public override void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        base.Awake();
    }

    
    public void DetermineWinner()
    {
        //Set finalscores to equal total of players
        finalScores = players;

        //sort list ordered by kills amt
        finalScores.Sort((x, y) => y.Kills.CompareTo(x.Kills));


        //How many final kill scores are there
        if(finalScores.Count > 1)
        {
            NumTiedPlayers = 1;

            for(int i = 0; i < finalScores.Count - 1; i++)
            {
                if(finalScores[i].Kills != finalScores[i + 1].Kills)
                {
                    Winner = finalScores[i].PlayerName;
                    break;
                }
                else
                {
                    NumTiedPlayers++;
                }
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Menu":

                menu = FindObjectOfType<Menu>();
                break;

            case "Lobby":

                lobbyUI = FindObjectOfType<LobbyUI>();

                    foreach (RoomPlayer p in roomPlayers)
                    {
                        if (p.hasAuthority)
                            p.ReloadLobby(false);
                    }

                

                break;

            case "Game":


                foreach (RoomPlayer p in roomPlayers)
                {
                    if (p.hasAuthority)
                    {
                        p.CmdGameStarted(true);
                        p.CmdSetReady();
                    }
                        
                }


                RoundSystem.roundTime = 10f;
                break;
        }
    }

    public void RefreshLobby()
    {
        for (int i = 0; i < maxConnections; i++)
        {
            if (i == 0)
                roomPlayers[i].IsHost = true;

            if (i < roomPlayers.Count)
            {
                lobbyUI.PlayerTeamImages[i].gameObject.SetActive(true);
                lobbyUI.PlayerStats[i].gameObject.SetActive(true);
                lobbyUI.PlayerReadyStatus[i].gameObject.SetActive(true);

                if (roomPlayers[i].AllInfoEntered)
                {
                    lobbyUI.PlayerNames[i].text = roomPlayers[i].PlayerName;

                    lobbyUI.PlayerStats[i].text = "Kills - " + roomPlayers[i].Kills + "Deaths -" + roomPlayers[i].Deaths;

                    if (roomPlayers[i].IsReady)
                    {
                        lobbyUI.PlayerReadyStatus[i].text = "READY!";
                        lobbyUI.PlayerReadyStatus[i].color = Color.green;
                    }
                    else
                    {
                        lobbyUI.PlayerReadyStatus[i].text = "UNREADY!";
                        lobbyUI.PlayerReadyStatus[i].color = Color.red;
                    }
                    lobbyUI.PlayerTeamImages[i].sprite = teamImages[roomPlayers[i].TeamIndex];
                }
                else
                {
                    lobbyUI.PlayerNames[i].text = "Player is finalising details.";
                    lobbyUI.PlayerTeamImages[i].sprite = null;
                }
            }
            else
            {
                lobbyUI.PlayerNames[i].text = "Waiting for players to join...";
                lobbyUI.PlayerTeamImages[i].gameObject.SetActive(false);
                lobbyUI.PlayerStats[i].gameObject.SetActive(false);
                lobbyUI.PlayerReadyStatus[i].gameObject.SetActive(false);
            }
            
        }
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers)
            return false;

        foreach (RoomPlayer p in roomPlayers)
        {
            if (!p.IsReady)
                return false;
        }

        return true;
    }

    public void NotifyPlayersReadyState()
    {
        foreach (RoomPlayer p in roomPlayers)
            p.ReadyToStart(IsReadyToStart());
    }

    void SetMap()
    {
        List<string> mapVotes = new List<string>();

        foreach (RoomPlayer rp in roomPlayers)
            mapVotes.Add(rp.MapVote);

        string topMap = mapVotes.GroupBy(x => x).OrderByDescending(y => y.Count()).First().Key;

        GameplayScene = topMap;
    }
        
    public void StartGame()
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            if (!IsReadyToStart())
                return;

            foreach (RoomPlayer rp in roomPlayers)
            {
                if (rp.MapVote == null)
                    return;
            }

            SetMap();

            ServerChangeScene(GameplayScene);
        }
    }

    public void ReturnToLobby()
    {
        if (NetworkServer.active && IsSceneActive(GameplayScene))
        {
            for(int i = 0; i < players.Count; i++)
            {
                roomPlayers[i].Kills = players[i].Kills;
                roomPlayers[i].Deaths = players[i].Deaths;
            }

            players.Clear();

            ServerChangeScene(RoomScene);
        }
    }

    //Overrides
    public override void OnRoomClientDisconnect(NetworkConnection conn)
    {
        if(IsSceneActive(offlineScene))
        {
            StartCoroutine(menu.TimeOutText());
        }
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        FirstPersonPlayer player = gamePlayer.GetComponent<FirstPersonPlayer>();
        RoomPlayer rPlayer = roomPlayer.GetComponent<RoomPlayer>();

        player.PlayerName = rPlayer.PlayerName;

        return true;
    }
}
