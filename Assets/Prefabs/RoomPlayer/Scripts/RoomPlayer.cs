using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class RoomPlayer : NetworkRoomPlayer
{
    private MyNetworkManager Room { get => NetworkManager.singleton as MyNetworkManager; }

    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject startButton;
    [SerializeField] RoomPlayerSetup playerSetUp;

    [SyncVar]
    private string playerName;
    public string PlayerName => playerName;

    [SyncVar]
    private int teamIndex;
    public int TeamIndex => teamIndex;

    [SyncVar]
    private bool isHost;

    [SyncVar]
    [SerializeField] private string mapVote;
    public string MapVote { get => mapVote; set => mapVote = value; }

    [SyncVar]
    [SerializeField] private int kills;
    public int Kills { get => kills; set => kills = value; }

    [SyncVar]
    [SerializeField] private int deaths;
    public int Deaths { get => deaths; set => deaths = value; }

    [SyncVar(hook = nameof(OnAllInfoEntered))]
    private bool allInfoEntered;
    public bool AllInfoEntered => allInfoEntered;

    [SyncVar(hook = nameof(OnReadyStatusChanged))]
    private bool isReady;
    public bool IsReady { get => isReady; set => isReady = value; }

    [SyncVar (hook = nameof(OnGameStarted))]
    [SerializeField] private bool gameStarted;
    public bool GameStarted { get => gameStarted; set => gameStarted = value; }
    public bool IsHost
    {
        set
        {
            isHost = value;
            startButton.SetActive(true);
        }
    }

    //Network Overrides
    public override void OnStartAuthority()
    {
        canvas.SetActive(true);
        playerSetUp.enabled = true;
        playerSetUp.RoomPlayerRef = this;
    }

    public override void OnStartClient()
    {
        Room.roomPlayers.Add(this);
    }

    //Methods
    public void ReadyToStart(bool ready)
    {
        if (!isHost)
            return;

        startButton.GetComponent<Button>().interactable = ready; 
    }

    public void ReloadLobby(bool started)
    {
        CmdGameStarted(started);

        Cursor.lockState = CursorLockMode.None;

        if (!started)
            Room.RefreshLobby();
    }

    

    //Commands
    [Command]
    public void CmdSetName(string name)
    {
        playerName = name;
    }

    [Command]
    public void CmdSetTeam(int team)
    {
        teamIndex = team;
    }

    [Command]
    public void CmdFinishSetUp()
    {
        allInfoEntered = true;
    }

    [Command]
    public void CmdSetMap(string map)
    {
        mapVote = map;
    }

    [Command]
    public void CmdSetReady()
    {
        isReady = !isReady;

        Room.NotifyPlayersReadyState();
    }

    [Command]
    public void CmdGameStarted(bool started)
    {
        GameStarted = started;
    }

    //SyncVar Hooks
    void OnAllInfoEntered(bool oldBool, bool newBool)
    {
        Room.RefreshLobby();
    }
    void OnReadyStatusChanged(bool oldBool, bool newBool)
    {
        Room.RefreshLobby();

        if (newBool)
        {
            playerSetUp.ReadyText.text = "Not Ready...";
        }
        else
        {
            playerSetUp.ReadyText.text = "Ready!";
        }
    }

    void OnGameStarted(bool oldBool, bool newBool) // Enables / Disables start game / ready buttons from lobby
    {
        bool b = !newBool;

        playerSetUp.ButtonGroup.SetActive(b);
    }


    //Remote Procedure Calls
    [ClientRpc]
    public void RpcHideButtons(bool visible)
    {
        if (!visible)
        {
            playerSetUp.ButtonGroup.SetActive(visible);
        }
    }
}
