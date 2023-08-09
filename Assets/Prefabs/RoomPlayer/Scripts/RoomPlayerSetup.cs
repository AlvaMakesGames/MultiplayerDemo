using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;

public class RoomPlayerSetup : MonoBehaviour
{
    private MyNetworkManager Room { get => NetworkManager.singleton as MyNetworkManager; }
    public RoomPlayer RoomPlayerRef { get; set; }

    [SerializeField] private TMP_Text readyText;
    public TMP_Text ReadyText { get => readyText; set => readyText = value; }

    [SerializeField] private GameObject buttonGroup;
    public GameObject ButtonGroup { get => buttonGroup; set => buttonGroup = value; }

    [SerializeField] private TMP_InputField nameInputField;    
    [SerializeField] private GameObject customisePanel;
    [SerializeField] private Image teamImage;

    [SerializeField] private GameObject mapVotePanel;

    private int teamNumber;

    private void Start()
    {
        UpdateTeamImage(0);
    }

    public void SetTeamLeft()
    {
        if (teamNumber - 1 == -1)
        {
            teamNumber = 1;
        }
        else
            teamNumber--;

        RoomPlayerRef.CmdSetTeam(teamNumber);
        UpdateTeamImage(teamNumber);
    }

    public void SetTeamRight()
    {
        if (teamNumber + 1 == 2)
        {
            teamNumber = 0;
        }
        else
            teamNumber++;

        RoomPlayerRef.CmdSetTeam(teamNumber);
        UpdateTeamImage(teamNumber);
    }

    void UpdateTeamImage(int team)
    {
        teamImage.sprite = Room.TeamImages[team]; 
    }

    public void ConfirmSetUp()
    {
        if (!string.IsNullOrEmpty(nameInputField.text))
        {
            RoomPlayerRef.CmdSetName(nameInputField.text);
            RoomPlayerRef.CmdFinishSetUp();
            customisePanel.SetActive(false);
        }
    }

    public void OpenMapVote()
    {
        if(!RoomPlayerRef.IsReady)
        {
            mapVotePanel.SetActive(true);
        }
        else
        {
            RoomPlayerRef.CmdSetMap(string.Empty);
            SetReadyStatus();
        }
    }

    public void SetReadyStatus()
    {
        RoomPlayerRef.CmdSetReady();
    }       

    public void StartGame()    
    {
        RoomPlayerRef.RpcHideButtons(false);
        Room.StartGame();
    }
}
