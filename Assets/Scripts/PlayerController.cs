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

    public Attack attack;

    private float moveH;
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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        AssignHealthBar();
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        if (photonView.IsMine)
        {
            if (LocalPlayerInstance != null) { LocalPlayerInstance = this.gameObject; }
            isFacingRight = true;
            PodeMover = true;
        }

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log((int)PhotonNetwork.LocalPlayer.CustomProperties["Score"]);
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        if (photonView.IsMine)
        {
            moveH = Input.GetAxis("Horizontal");

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
            if (PodeMover)
            {
                photonView.RPC("WeakAttack", RpcTarget.All);
            }

        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (PodeMover)
            {
                photonView.RPC("StrongAttack", RpcTarget.All);
            }

        }
        }
    }

    private void AssignHealthBar()
    {
        GameObject canvas = GameObject.Find("Canvas"); // Ensure the Canvas exists

        if (photonView.IsMine)
        {
            // The local player should always see their health in "Player1HealthBar"
            myHealthBar = canvas.transform.Find("Player1HealthBar")?.GetComponent<Image>();
        }
        else
        {
            // The remote player should always be linked to "Player2HealthBar"
            myHealthBar = canvas.transform.Find("Player2HealthBar")?.GetComponent<Image>();
        }

        if (myHealthBar != null)
        {
            Debug.Log(photonView.Owner.NickName + " assigned " + myHealthBar.name);
            UpdateHealthUI();
        }
        else
        {
            Debug.LogError("Health bar not found for " + photonView.Owner.NickName);
        }
    }

    private void UpdateHealthUI()
    {
        Debug.Log("Current Health: " + currentHealth);
        if (myHealthBar != null)
        {
            myHealthBar.fillAmount = currentHealth / maxHealth;
            Debug.Log("Health fill amount: " + myHealthBar.fillAmount);
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
            UpdateHealthUI();

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
        UpdateHealthUI();
        if (currentHealth <= 0)
        {
            string playFabId = PhotonNetwork.LocalPlayer.UserId;
            if (photonView.IsMine)
            {
                photonView.RPC("UpdateLeaderboard", RpcTarget.Others);
            }
                
            photonView.RPC("Die", RpcTarget.All);
            return;
        }

        _anim.SetTrigger("Damaged");

        //_rb.velocity = new Vector2(hitDirection.x * knockbackForce, _rb.velocity.y);

        StartCoroutine(HitCoroutine(hitStunDuration));
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
    public void UpdateLeaderboard()
    {
        UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {

                    StatisticName = "Kills",
                    Value =+ 1
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(
            request,
            result =>
            {
                Debug.Log("[Playfab] Leaderboard foi atualizado!");
            },
            error =>
            {
                Debug.LogError($"[PlayFab] {error.GenerateErrorReport()}");
            }
        );
    }
}

