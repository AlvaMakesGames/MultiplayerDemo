using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class LobbyUI : MonoBehaviour
{
    private MyNetworkManager Room { get => NetworkManager.singleton as MyNetworkManager; }

    //Var
    [SerializeField] private TMP_Text[] playerNames;
    [SerializeField] private TMP_Text[] playerReadyStatus;
    [SerializeField] private Image[] playerTeamImages;
    [SerializeField] private TMP_Text[] playerStats;

    //Properties
    public TMP_Text[] PlayerNames { get => playerNames; set => playerNames = value; }
    public TMP_Text[] PlayerReadyStatus { get => playerReadyStatus; set => playerReadyStatus = value; }
    public Image[] PlayerTeamImages { get => playerTeamImages; set => playerTeamImages = value; }
    public TMP_Text[] PlayerStats { get => playerStats; set => playerStats = value; }
}
