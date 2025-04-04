using System;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
//using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.ComponentModel;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Fields
    private int lastHealth = -1;
    public Image myHealthBar;
    public Transform groundCheck;  // Assign in Inspector (place it near player's feet)
    public LayerMask groundLayer;  // Assign this in Inspector to "Terrain"
    public bool playerIsGrounded;
    private bool isJumpPressed;
    private bool isDead = false;
    public bool isHit;
    public static PlayerController Instance;
    public static GameObject LocalPlayerInstance;
    private Animator _anim;
    private Rigidbody2D _rb;
    private TMP_Text _namePlayer;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _playerSpeed = 10f;
    private Vector3 networkPosition;
    private string _nickname;
    private bool isFacingRight = true;

    public bool invincible = false;
    public bool playerIsCrouching;
    public GameManager gm;
    public Attack attack;
    private bool canJump = true;
    public Transform opponent;
    private float moveH;
    private float moveV;
    private int _localScore;

    public int maxHealth = 10;
    [SerializeField] public int currentHealth;

    [SerializeField] private float knockbackForce = 5f;
    public bool PodeMover { get; private set; }

    public bool flipped;

    #endregion

    #region Properties

    public Vector3 Movement { get; set; }
    public float JumpForce => _jumpForce;

    #endregion

    public void HabilitaMovimentacao(bool mover)
    {
        PodeMover = mover;
    }

    // Start is called before the first frame update
    void Start()
    {
        invincible = false;
        isDead = false;
        StartCoroutine(FindOpponent());
        Debug.Log($"[Photon] Player Start. ViewID: {photonView.ViewID} | IsMine: {photonView.IsMine}");
        AssignHealthBar();
        gm = GameObject.Find("GameManager")?.GetComponent<GameManager>();
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        currentHealth = maxHealth;

        if (photonView.IsMine)
        {
            isFacingRight = true;
            PodeMover = true;
        }

    }

    //public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    //{
    //    Debug.Log((int)PhotonNetwork.LocalPlayer.CustomProperties["Score"]);
    //}

    // Update is called once per frame
    void Update()
    {
        playerIsGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        _anim.SetBool("Jumping", !playerIsGrounded);

        if (photonView.IsMine)
        {
            moveH = Input.GetAxis("Horizontal");
            moveV = Input.GetAxis("Vertical");

            // Only allow jumping if the player is on the ground
            if (canJump && Input.GetButtonDown("Jump") && playerIsGrounded)
            {
                isJumpPressed = true;
                

            }
        }

        if (photonView.IsMine)
        {
            photonView.RPC("MovementAnim", RpcTarget.All, moveH);
        }

        _anim.SetFloat("yVel",_rb.velocity.y);
        //INPUTS
        if (!playerIsCrouching && moveV <= -0.1f && playerIsGrounded)
        {
            if (playerIsCrouching) return;
            HabilitaMovimentacao(false);
            playerIsCrouching = true;
            canJump = false;
            _rb.velocity = Vector2.zero;
            photonView.RPC("Crouch", RpcTarget.All);

        }
        if (playerIsCrouching && moveV >= -0.1f && playerIsGrounded)
        {
            if (!playerIsCrouching) return;
            HabilitaMovimentacao(true);
            playerIsCrouching = false;
            canJump = true;
            photonView.RPC("Stand", RpcTarget.All);

        }

        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (PodeMover && playerIsGrounded)
                {
                    photonView.RPC("WeakAttack", RpcTarget.All);
                }
                else if(PodeMover && !playerIsGrounded)
                {
                    photonView.RPC("JumpWeakAttack", RpcTarget.All);
                }
                else if(playerIsCrouching && playerIsGrounded)
                {
                    photonView.RPC("CrouchWeakAttack", RpcTarget.All);
                }

            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                if (PodeMover && playerIsGrounded)
                {
                    photonView.RPC("StrongAttack", RpcTarget.All);
                }

            }
               
        }
    }

    public void AssignHealthBar()
    {
        GameObject canvas = GameObject.Find("Canvas"); // Ensure the Canvas exists

        // Assign health bar based on the owner's actor number (not the local player!)
        if (photonView.Owner.ActorNumber == 1)
        {
            myHealthBar = canvas.transform.Find("Player1HealthBar")?.GetComponent<Image>();
        }
        else
        {
            myHealthBar = canvas.transform.Find("Player2HealthBar")?.GetComponent<Image>();
        }

        if (myHealthBar != null)
        {
            Debug.Log($"{photonView.Owner.NickName} assigned {myHealthBar.name}");
        }
        else
        {
            Debug.LogError($"Health bar not found for {photonView.Owner.NickName}");
        }
    }


    [PunRPC]
    void UpdateHealthBar(int newHealth)
    {
        Debug.Log($"[UpdateHealthUI] Called for {photonView.Owner.NickName} by {PhotonNetwork.LocalPlayer.NickName}, isMine: {photonView.IsMine}, new health: {newHealth}");
        Debug.Log("[UpdateHealthUI] Stack Trace:\n" + Environment.StackTrace);
        if (newHealth == lastHealth) return; // Prevent unnecessary updates
        lastHealth = newHealth;
        myHealthBar.fillAmount = (float)newHealth / maxHealth;
    }


    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            Debug.Log("OPP: " + opponent);
            if (PodeMover)
            {
                Debug.Log("PodeMover");
                if (opponent != null)
                {
                    if (transform.position.x > opponent.position.x && !flipped)
                    {
                        Debug.Log("flip called - facing left");
                        photonView.RPC("Flip", RpcTarget.All);
                        flipped = true;
                    }
                    else if (transform.position.x < opponent.position.x && flipped)
                    {
                        Debug.Log("flip called - facing right");
                        photonView.RPC("Flip", RpcTarget.All);
                        flipped = false;
                    }
                }
            }
            Vector2 Movement = new(moveH * _playerSpeed, _rb.velocity.y);

            if (isJumpPressed && playerIsGrounded) // Ensures jump only when grounded
            {
                _rb.velocity = new Vector2(_rb.velocity.x, JumpForce);
                isJumpPressed = false; // Reset jump input
            }

            if (PodeMover)
            {
                Debug.Log("Pode Mover");
                _rb.velocity = new Vector2(Movement.x, _rb.velocity.y);
            }
            else
            {
                Debug.Log("Não Pode Mover");
            }
           
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
        }



    }



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((Vector3)transform.position);
            stream.SendNext(_nickname);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            _nickname = (string)stream.ReceiveNext();

            HandlePlayerDeath();
            //_namePlayer.text = _nickname;
        }


    }

    [PunRPC]
    public void Flip()
    {

            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        
    }

    [PunRPC]
    private void MovementAnim(float moveH)
    {
        _anim.SetFloat("xVel", Math.Abs(moveH));
    }

    public void Hit(float duration)
    {
        if(!isHit)
        {
            StartCoroutine(HitCoroutine(duration));
        }
    }

    private IEnumerator HitCoroutine(float duration)
    {
        isHit = true;
        PodeMover = false;
        yield return new WaitForSeconds(duration);

        isHit = false;
        PodeMover = true;
    }

    [PunRPC]
    public void TakeDamage(int damage, Vector2 hitDirection, float hitStunDuration, bool enemyIsGrounded, bool enemyIsCrouching)
    {
        Debug.Log("isdead" + isDead);
        Debug.Log("invincible" + invincible);
        Debug.Log($"TakeDamage called for {photonView.Owner.NickName}, photonView.IsMine: {photonView.IsMine}, Current Health: {currentHealth}");

        if (isDead)
        {
            Debug.Log($"TakeDamage ignored - {photonView.Owner.NickName} is Dead.");
            return;
        }
        if (invincible)
        {
            Debug.Log($"TakeDamage ignored - {photonView.Owner.NickName} is Dead.");
            return;
        }

        bool isBlocked = (moveH <= -0.1f && hitDirection.x >= 0) || (moveH >= 0.1f && hitDirection.x <= 0);

        if (isBlocked)
        {
                // Blocking Scenarios
                if ((playerIsCrouching && enemyIsCrouching) ||  // Both crouching → Blocked
                    (!playerIsCrouching && !enemyIsCrouching && enemyIsGrounded && playerIsGrounded) || // Both standing and not crouching → Blocked
                    (!playerIsGrounded && !enemyIsGrounded) || // Both airborne → Blocked
                    (!playerIsCrouching && playerIsGrounded && !enemyIsGrounded)) // Player standing (not crouching), enemy airborne → Blocked
                {
                    Debug.Log($"Attack BLOCKED by {photonView.Owner.NickName}! Health remains {currentHealth}");

                    if (playerIsCrouching)
                        photonView.RPC("CrouchBlock", RpcTarget.All);
                    else if (!playerIsCrouching && playerIsGrounded)
                        photonView.RPC("Block", RpcTarget.All);
                    else if (!playerIsGrounded)
                        photonView.RPC("JumpBlock", RpcTarget.All);

                    Debug.Log($"[TakeDamage] Exiting early due to block!");
                    return; // ✅ Exit function
                }
        }

        // Damage calculation should only run on the player who owns this PhotonView
        if (photonView.IsMine)
        {
            int previousHealth = currentHealth;
            currentHealth -= damage;
            Debug.Log($"[TakeDamage] Health changed from {previousHealth} to {currentHealth}");
            photonView.RPC("UpdateHealthBar", RpcTarget.All, currentHealth);
            if(currentHealth <= 0)
            {
                HandlePlayerDeath();
            }
        }

        // ✅ Play animation and effects on all clients
        photonView.RPC("DamageAnim", RpcTarget.All);
    }

    [PunRPC]
    void DamageAnim()
    {
        _anim.SetTrigger("Damaged");
        Debug.Log("[TakeDamage] Played damage animation for everyone!");
    }


    private void HandlePlayerDeath()
    {
        // Check if the player is dead
        if (currentHealth <= 0)
        {
            // The player is dead. Handle death logic.


            // Trigger death animations, invincibility, disable movement, etc.
            photonView.RPC("Die", RpcTarget.All);

            // Update the killer’s stats (if applicable)
            // This logic would depend on how the game identifies the killer.
            if (photonView.IsMine)
            {
                string playerID = PhotonNetwork.LocalPlayer.UserId; // Get the player's ID (use for logging or other logic)
                Debug.Log($"{playerID} is dead");
                // The player calling this function is the one who killed someone
                photonView.RPC("UpdateStatsAfterKill", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void UpdateStatsAfterKill()
    {
        if (photonView.IsMine)
        {
            // Get the player who died (the other player)
            UpdatePlayerStatsForDeadPlayer();
        }
        else
        {   
            // Get the player who killed (the player who called this RPC)
            UpdatePlayerStatsForKiller();

        }
    }
    private void UpdatePlayerStatsForKiller()
{
    // Get the current stats (Kills, Deaths, KD) for the player who killed
    GetPlayerStatisticsRequest statsRequest = new GetPlayerStatisticsRequest();
        int rewardAmount = 100;
        int killAmount = 50;
    PlayFabClientAPI.GetPlayerStatistics(
        statsRequest,
        statsResult =>
        {
            int currentKills = 0;
            int currentDeaths = 0;

            // Check the player's current statistics (Kills, Deaths)
            foreach (var stat in statsResult.Statistics)
            {
                if (stat.StatisticName == "Kills")
                {
                    currentKills = stat.Value;
                }
                else if (stat.StatisticName == "Deaths")
                {
                    currentDeaths = stat.Value;
                }
            }

            // Increment Kills for the killer
            int newKills = currentKills + 1;

            // Calculate the new KD for the killer
            float newKD = (currentDeaths > 0) ? (newKills / (float)currentDeaths) : newKills;

                       // Check if the player's kills are a multiple of 2 (and greater than 0)
                        if (newKills > 0 && newKills % 2 == 0)
                        {
                            // Grant virtual currency (100 Battle Coins) for each multiple of 2 kills
                            GrantCurrencyReward("BC", rewardAmount);
                            Debug.Log($"[PlayFab] Player rewarded with {rewardAmount} BC for reaching {newKills} kills.");
                        }
            GrantCurrencyReward("BC", killAmount);

            // Now update the killer's statistics (Kills, Deaths, and KD)
            UpdatePlayerStatisticsRequest updateRequest = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate
                    {
                        StatisticName = "Kills",
                        Value = newKills
                    },
                    new StatisticUpdate
                    {
                        StatisticName = "Deaths",
                        Value = currentDeaths
                    },
                    new StatisticUpdate
                    {
                        StatisticName = "KD",
                        Value = (int)Math.Round(newKD * 100)  // Scaling KD for better precision
                    }
                }
            };

            // Update the player's stats on PlayFab
            PlayFabClientAPI.UpdatePlayerStatistics(
                updateRequest,
                result => 
                {
                    Debug.Log("[PlayFab] Killer's statistics updated!");
                },
                error => 
                {
                    Debug.LogError($"[PlayFab] Error updating killer's statistics: {error.GenerateErrorReport()}");
                }
            );
        },
        error =>
        {
            Debug.LogError($"[PlayFab] Error getting killer's statistics: {error.GenerateErrorReport()}");
        }
    );
}

private void UpdatePlayerStatsForDeadPlayer()
{
    // Get the current stats (Kills, Deaths, KD) for the player who died
    GetPlayerStatisticsRequest statsRequest = new GetPlayerStatisticsRequest();
        int deathAmount = 25;
        PlayFabClientAPI.GetPlayerStatistics(
        statsRequest,
        statsResult =>
        {
            int currentKills = 0;
            int currentDeaths = 0;

            // Check the player's current statistics (Kills, Deaths)
            foreach (var stat in statsResult.Statistics)
            {
                if (stat.StatisticName == "Kills")
                {
                    currentKills = stat.Value;
                }
                else if (stat.StatisticName == "Deaths")
                {
                    currentDeaths = stat.Value;
                }
            }

            // Increment Deaths for the player who died
            int newDeaths = currentDeaths + 1;

            // Calculate the new KD for the dead player
            float newKD = (currentKills > 0) ? (currentKills / (float)newDeaths) : 0;  // Avoid division by zero


            GrantCurrencyReward("BC", deathAmount);


            // Now update the dead player's statistics (Kills, Deaths, and KD)
            UpdatePlayerStatisticsRequest updateRequest = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate
                    {
                        StatisticName = "Deaths",
                        Value = newDeaths
                    },
                    new StatisticUpdate
                    {
                        StatisticName = "Kills",
                        Value = currentKills
                    },
                    new StatisticUpdate
                    {
                        StatisticName = "KD",
                        Value = (int)Math.Round(newKD * 100) // Scaling KD for better precision
                    }
                }
            };

            // Update the player's stats on PlayFab
            PlayFabClientAPI.UpdatePlayerStatistics(
                updateRequest,
                result => 
                {
                    Debug.Log("[PlayFab] Dead player's statistics updated!");
                },
                error => 
                {
                    Debug.LogError($"[PlayFab] Error updating dead player's statistics: {error.GenerateErrorReport()}");
                }
            );
        },
        error =>
        {
            Debug.LogError($"[PlayFab] Error getting dead player's statistics: {error.GenerateErrorReport()}");
        }
    );
}
    [PunRPC]
    public void Die()
    {
        if (isDead) return; // Prevent multiple executions
        isDead = true;
        invincible = true;
        Debug.Log("Die invincibility");
        Debug.Log("Die movement");
        PodeMover = false;
        _anim.SetTrigger("Defeat");
        _anim.SetBool("Alive", false);
        Debug.Log("Die anim trigger");
        gm.GameOver();
    }


    [PunRPC]
    public void Block()
    {
        _anim.SetTrigger("Block");
    }
    [PunRPC]
    public void JumpBlock()
    {
        _anim.SetTrigger("JumpBlock");
    }
    [PunRPC]
    public void CrouchBlock()
    {
        _anim.SetTrigger("CrouchBlock");
    }
    [PunRPC]
    public void WeakAttack()
    {
        _anim.SetTrigger("WeakA");
        attack.damage = 1;
        attack.hitStunDuration = 0.25f;
    }
    [PunRPC]
    public void StrongAttack()
    {
        _anim.SetTrigger("StrongA");
        attack.damage = 1;
        attack.hitStunDuration = 0.50f;
    }

    [PunRPC]
    public void JumpWeakAttack()
    {
        _anim.SetTrigger("JumpWeakA");
        attack.damage = 1;
        attack.hitStunDuration = 0.25f;
    }

    [PunRPC]
    public void CrouchWeakAttack()
    {
        _anim.SetTrigger("CrouchWeakA");
        attack.damage = 1;
        attack.hitStunDuration = 0.25f;
    }

    [PunRPC]
    public void Crouch()
    {
        _anim.SetBool("Crouch",true);
    }

    [PunRPC]
    public void Stand()
    {
        _anim.SetBool("Crouch",false);
    }

    [PunRPC]
    public void Jumping()
    {
        _anim.SetTrigger("Jump");
    }

    public void GrantCurrencyReward(string currencyCode, int amount)
    {
        // Grant virtual currency (e.g., BC = Battle Coins)
        AddUserVirtualCurrencyRequest currencyRequest = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = currencyCode,
            Amount = amount
        };

        PlayFabClientAPI.AddUserVirtualCurrency(
            currencyRequest,
            result => { Debug.Log($"[PlayFab] {currencyCode} granted: {amount}"); },
            error => { Debug.LogError($"[PlayFab] {error.GenerateErrorReport()}"); }
        );
    }
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (photonView.IsMine)
        {
            Debug.Log("[Photon] This player controls this instance.");
        }
        else
        {
            Debug.Log("[Photon] This instance is controlled by another player.");
        }
    }

    IEnumerator FindOpponent()
    {
        while (opponent == null) // Keep checking until an opponent is found
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject p in players)
            {
                PhotonView pv = p.GetComponent<PhotonView>();
                if (!pv.IsMine) // Ensure it's the opponent
                {
                    opponent = p.transform;
                    Debug.Log("Opponent assigned: " + opponent.name);
                    yield break; // Exit loop once the opponent is found
                }
            }
            yield return new WaitForSeconds(0.5f); // Wait and try again
        }
    }
}

