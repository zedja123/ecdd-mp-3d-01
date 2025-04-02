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

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Fields
    public Image myHealthBar;
    public Transform groundCheck;  // Assign in Inspector (place it near player's feet)
    public LayerMask groundLayer;  // Assign this in Inspector to "Terrain"
    private bool isGrounded;
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
    private bool agachado;
    public GameManager gm;
    public Attack attack;

    private float moveH;
    private float moveV;
    private int _localScore;

    public int maxHealth = 10;
    [SerializeField] private int currentHealth;

    [SerializeField] private float knockbackForce = 5f;
    public bool PodeMover { get; private set; }

    #endregion

    #region Properties

    public Vector3 Movement { get; set; }
    public float JumpForce => _jumpForce;
    public float PlayerSpeed
    {
        get => _playerSpeed;
        set => _playerSpeed = value;
    }

    #endregion

    public void HabilitaMovimentacao(bool mover)
    {
        PodeMover = mover;
    }

    // Start is called before the first frame update
    void Start()
    {
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
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        if (photonView.IsMine)
        {
            moveH = Input.GetAxis("Horizontal");
            moveV = Input.GetAxis("Vertical");

            // Only allow jumping if the player is on the ground
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                isJumpPressed = true;
            }
        }

        if (photonView.IsMine)
        {
            photonView.RPC("MovementAnim", RpcTarget.All, moveH);
        }

        //INPUTS
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (PodeMover && isGrounded)
                {
                    photonView.RPC("WeakAttack", RpcTarget.All);
                }
                else if(PodeMover && !isGrounded)
                {

                }
                else if(agachado && isGrounded)
                {

                }

            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                if (PodeMover && isGrounded)
                {
                    photonView.RPC("StrongAttack", RpcTarget.All);
                }

            }


           if (!agachado && moveV <= -0.1f && isGrounded)
           {
                HabilitaMovimentacao(false);
                photonView.RPC("Crouch", RpcTarget.All);
                agachado = true;
           }
           if (agachado && moveV >= -0.1f && isGrounded)
           {
                HabilitaMovimentacao(true);
                photonView.RPC("Stand", RpcTarget.All);
                agachado = false;
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
    public void UpdateHealthUI(int health)
    {
        if (myHealthBar != null)
        {
            float healthRatio = (float)health / maxHealth;
            myHealthBar.fillAmount = healthRatio;
            Debug.Log($"{photonView.Owner.NickName} updated health: {healthRatio}");
        }
    }


    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            Vector2 Movement = new Vector2(moveH * PlayerSpeed, _rb.velocity.y);

            if (isJumpPressed && isGrounded) // Ensures jump only when grounded
            {
                _rb.velocity = new Vector2(_rb.velocity.x, JumpForce);
                isJumpPressed = false; // Reset jump input
            }

            if (PodeMover)
            {
                _rb.velocity = new Vector2(Movement.x, _rb.velocity.y);
            }
            if (PodeMover)
            {
                photonView.RPC("Flip", RpcTarget.All);
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
            stream.SendNext(currentHealth);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            _nickname = (string)stream.ReceiveNext();
            currentHealth = (int)stream.ReceiveNext();

            HandlePlayerDeath();
            //_namePlayer.text = _nickname;
        }


    }

    [PunRPC]
    private void MovementRPC(float jump)
    {
        Movement = new Vector2(moveH * PlayerSpeed, jump);
    }
    [PunRPC]
    private void Flip()
    {
        if (isFacingRight && moveH < 0f || !isFacingRight && moveH > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    [PunRPC]
    private void MovementAnim(float moveH)
    {
        _anim.SetFloat("Velocity", Math.Abs(moveH));
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
    public void TakeDamage(int damage, Vector2 hitDirection, float hitStunDuration)
    {
        if(invincible) return;
        Debug.Log("Hit");
        currentHealth -= damage;
        photonView.RPC("UpdateHealthUI", RpcTarget.All, currentHealth); // ✅ Send health data
        if (currentHealth <= 0)
        {
            // This player is dead, check who killed them
            HandlePlayerDeath();
            return;
        }

        _anim.SetTrigger("Damaged");

        //_rb.velocity = new Vector2(hitDirection.x * knockbackForce, _rb.velocity.y);

        StartCoroutine(HitCoroutine(hitStunDuration));
    }

    private void HandlePlayerDeath()
    {
        // Check if the player is dead
        if (currentHealth <= 0)
        {
            // The player is dead. Handle death logic.


            // Trigger death animations, invincibility, disable movement, etc.
            Die();

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
        HabilitaMovimentacao(false);
        Debug.Log("Die movement");
        _anim.SetTrigger("Defeat");
        _anim.SetBool("Alive", false);
        Debug.Log("Die anim trigger");
        gm.GameOver();
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
    public void JumpAttack()
    {
        _anim.SetTrigger("JumpA");
        attack.damage = 1;
        attack.hitStunDuration = 0.25f;
    }

    [PunRPC]
    public void Crouch()
    {
        _anim.SetTrigger("Crouch");
    }

    [PunRPC]
    public void Stand()
    {
        _anim.SetTrigger("Stand");
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
}

