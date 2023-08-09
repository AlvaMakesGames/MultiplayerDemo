using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Linq;

public class FirstPersonPlayer : NetworkBehaviour
{
    private MyNetworkManager Room { get => NetworkManager.singleton as MyNetworkManager; }

    [Header("Objects")]
    [SerializeField] private Transform cameraParent;    
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPt;
    [SerializeField] private GameObject[] guns;

    [Header("UI")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private Image healthBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private TMP_Text killsText, deathsText;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text[] endGameDetails;

    [Header("Chat")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_Text chatOutput;
    [SerializeField] private TMP_InputField chatInput;
    private TMP_Text[] playerDetails;

    //Movement
    #region Movement
    private CharacterController controller;
    private float speed = 5f;
    private float gravity = 20f;
    private float jumpForce = 10f;
    private Vector3 vertVel;
    private bool toggleCursor;
    #endregion

    //Player Health
    #region Player Health
    private const int maxHealth = 100;
    [SyncVar (hook = nameof(OnHealthChanged))] 
    [SerializeField] private int health;
    #endregion

    //Weapons
    #region Weapons
    [SyncVar (hook = nameof(OnWeaponChanged))]
    private int weapon;
    private Gun currentGun;
    #endregion

    //Player Details
    #region PlayerDetails
    [SyncVar (hook= nameof(OnKillsChanged))] private int kills;
    public int Kills { get => kills; set => kills = value; }

    [SyncVar (hook =nameof(OnDeathsChanged))] private int deaths;
    public int Deaths { get => deaths; set => deaths = value; }
   
    [SyncVar] private string playerName;
    public string PlayerName { get => playerName; set => playerName = value; }

    //Round Details
    [SyncVar (hook = nameof(OnRoundEnded))] private bool roundEnded;
    public bool RoundEnded { get => roundEnded; set => roundEnded = value; }

    private string[] numbers = new string[]
        {
            "zero",
            "one",
            "two",
            "three",
            "four"
        };

    #endregion

    //Chat
    private static event Action<string> OnChat;
    private bool chatOpen;
    public bool ChatOpen => chatOpen;

    //Unity Callbacks and Methods
    void Start ()
    {
        controller = GetComponent<CharacterController>();

        playerDetails = scorePanel.GetComponentsInChildren<TMP_Text>();

        Cursor.lockState = CursorLockMode.Locked;        
	}
	
	void Update ()
    {
        if (roundEnded)
            return;

        UpdateTimer();
        
        ToggleChatPanel();

        if (!chatOpen)
        {
            Movement();
            SwitchWeapon();
            Shoot();
            ToggleScorePanel();
        }
	}    

    void InitialiseLocalPlayer()
    {
        enabled = true;
        canvas.SetActive(true);
        OnChat += UpdateChat;

        foreach (Behaviour b in cameraParent.GetComponents<Behaviour>())
            b.enabled = true;
    }

    void ToggleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            toggleCursor = !toggleCursor;

        if (toggleCursor)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }

    void Movement()
    {
        //Calculate X/Z Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 movement = transform.right * x + transform.forward * z;
        controller.Move(movement * speed * Time.deltaTime);

        //Calculate Y Movement
        if (IsGrounded())
        {
            if (Input.GetKeyDown(KeyCode.Space))
                vertVel.y = jumpForce;
        }
        else
        {
            vertVel.y -= gravity * Time.deltaTime;
        }

        controller.Move(vertVel * Time.deltaTime);       
    }

    void ToggleScorePanel()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!scorePanel.activeSelf)
            {
                scorePanel.SetActive(true);

                RefreshScorePanel();
            }
            else
            {
                scorePanel.SetActive(false);
            }
        }
    }

    void ToggleChatPanel()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!chatOpen)
            {
                chatPanel.SetActive(true);
                chatOpen = true;
            }
            else
            {
                chatPanel.SetActive(false);
                chatOpen = false;
            }
        }

        if (chatOpen)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }

    void RefreshScorePanel()
    {
        for (int i = 0; i < Room.maxConnections; i++)
        {
            if (i < Room.players.Count)
            {
                playerDetails[i].text = Room.players[i].playerName
                    + " -- Kills " + Room.players[i].kills
                    + " -- Deaths " + Room.players[i].deaths;
            }
            else
            { 
                playerDetails[i].text = "---"; 
            }
        }
    }

    bool IsGrounded()
    {
        Ray ray = new Ray(new Vector3(controller.bounds.center.x, (controller.bounds.center.y - controller.bounds.extents.y), controller.bounds.center.z), Vector3.down);
        return (Physics.Raycast(ray, 0.3f));
    }

    void UpdateTimer()
    {
        string timer = string.Format("{0}:{1}", Mathf.Floor(RoundSystem.roundTime / 60).ToString("00"), Mathf.Floor(RoundSystem.roundTime % 60).ToString("00"));
        timerText.text = timer;
    }

    void UpdateChat(string message)
    {
        //Validation goes here if needed.
        chatOutput.text += message;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Collectable item))
        {
            item = other.GetComponent<Collectable>();
        }

        switch (item.tag)
        {
            case "HealthPack":
                AddHealth(item.AmtToAdd, item);
                break;
        }
    }

    //Network Overrides
    public override void OnStartAuthority()
    {
        InitialiseLocalPlayer();
    }

    public override void OnStartClient()
    {
        Room.players.Add(this);      
        health = maxHealth;
        OnWeaponChanged(0, 0);
        OnKillsChanged(0, 0);
        OnDeathsChanged(0, 0);
    }

    //Server-side
    [Server]
    public void TakeDamage(FirstPersonPlayer owner)
    {
        int damage = UnityEngine.Random.Range(10, 16);

        if (health - damage < 0)
            health = 0;
        else
            health -= damage;

        if(health <= 0)
        {
            deaths++;

            if (owner != this)
                owner.Kills++;

            health = maxHealth;
            RpcRespawn();
        }
    }

    [Server]
    public void AddHealth(int amt, Collectable item)
    {
        if (health == maxHealth)
            return;

        if (health + amt > maxHealth)
            health = maxHealth;
        else
            health += amt;

        item.RpcHideItem();
    }

    //Client-side
    [Client]
    void Shoot()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            switch (currentGun.WeaponType)
            {
                case Type.Projectile:
                    
                    CmdShootProjectile();
                    
                    break;
                case Type.Hitscan:

                    if(Physics.Raycast(bulletSpawnPt.position, bulletSpawnPt.forward, out RaycastHit hit, currentGun.Range))
                    {
                        if(hit.collider.CompareTag("Player"))
                        {
                            FirstPersonPlayer hitPlayer = hit.collider.GetComponent<FirstPersonPlayer>();
                            CmdShootHitscan(hitPlayer);
                        }
                    }
                    break;
            }
        }
    } 
    
    [Client]
    void SwitchWeapon()
    {
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");

        CmdSwitchWeapon(scroll);
    }

    [Client]
    public void SendChat(string message)
    {
        if (!Input.GetKeyDown(KeyCode.Return))
            return;

        if (string.IsNullOrWhiteSpace(message))
            return;

        CmdSendChat(chatInput.text);

        chatInput.text = string.Empty;
    }

    [Client]

    private void OnDestroy()
    {
        if (hasAuthority)
            OnChat -= UpdateChat;
    }

    [Client]
    void OpenEndPanel()
    { 
        gameplayPanel.SetActive(false);
        endGamePanel.SetActive(false);

        for(int i = 0; i < Room.maxConnections; i++)
        {
            if (i < Room.players.Count)
                endGameDetails[i].text = Room.finalScores[i].PlayerName + "-- Kills - " + Room.finalScores[i].Kills;
            else
                endGameDetails[i].text = "---";
        }
    }


    //Commands
    [Command]
    void CmdShootProjectile()
    {
        GameObject go = Instantiate(bulletPrefab, bulletSpawnPt.position, bulletSpawnPt.rotation);
        go.GetComponent<Bullet>().Owner = this;
        NetworkServer.Spawn(go);
    }

    [Command]
    void CmdShootHitscan(FirstPersonPlayer hit)
    {
        hit.TakeDamage(this);
    }

    [Command]
    void CmdSwitchWeapon(float scroll)
    {
        if(scroll > 0)
        {
            if (weapon + 1 == guns.Length)
                weapon = 0;
            else
                weapon++;
        }

        if(scroll < 0)
        {
            if (weapon - 1 < 0)
                weapon = guns.Length - 1;
            else
                weapon--;
        }
    }

    [Command]
    void CmdSendChat(string message)
    {
        //validation
        string newMsg = message;
        if (newMsg.Contains("mobile"))
            newMsg.Replace("mobile", "*******");

        RPCSendChat($"[{playerName}]: {newMsg}");
    }

    //Remote Procedure Calls
    [ClientRpc]
    void RpcRespawn()
    {
        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
    }

    [ClientRpc]
    void RPCSendChat(string message)
    {
        OnChat?.Invoke($"\n{message}");
    }

    //SyncVar Hooks
    void OnHealthChanged(int oldHealth, int newHealth)
    {
        healthBar.fillAmount = (float)newHealth / 100;
        healthText.text = newHealth.ToString() + "%";
    }

    void OnWeaponChanged(int oldWeapon, int newWeapon)
    {
        //Sets the gun to be active/inactive based on current weapon index
        for(int i = 0; i < guns.Length; i++)
        {
            guns[i].SetActive(i == newWeapon);
        }

        //Assign values to the spawn point and current gun
        bulletSpawnPt = guns[newWeapon].transform.Find("spawnPoint");
        currentGun = guns[newWeapon].GetComponent<Gun>();
    }

    void OnKillsChanged(int oldKills, int newKills)
    {
        if (scorePanel.activeSelf)
            RefreshScorePanel();

        killsText.text = "Kills: " + newKills;
    }

    void OnDeathsChanged(int oldDeaths, int newDeaths)
    {
        if (scorePanel.activeSelf)
            RefreshScorePanel();

        deathsText.text = "Deaths: " + newDeaths;
    }

    void OnRoundEnded(bool oldBool, bool newBool)
    {
        if (newBool)
            StartCoroutine(EndGameRoutine());
    }

    //Coroutines
    IEnumerator EndGameRoutine()
    {
        //Determine player score/winner.

        Room.DetermineWinner();

        if (Room.finalScores.Count == 1)
            winnerText.text = "You are the only player";
        else
        {
            if (Room.NumTiedPlayers < 1)
                winnerText.text = "Top" + numbers[Room.NumTiedPlayers] + "are tied";
            else
                winnerText.text = Room.Winner + "has won the round";
        }

        OpenEndPanel();

        yield return new WaitForSeconds(5);

        Room.ReturnToLobby();
    }
}
