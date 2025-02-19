using System;
using System.Collections;

//using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Fields
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
        bool isJumpPressed = Input.GetButtonDown("Jump");
        float jump = isJumpPressed ? _rb.velocity.y + JumpForce : _rb.velocity.y;
        Movement = new Vector2(moveH * PlayerSpeed, jump);
        Animations();


        //INPUTS
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (PodeMover)
            {
                photonView.RPC("WeakAttack", RpcTarget.AllBuffered);
            }

        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (PodeMover)
            {
                photonView.RPC("StrongAttack", RpcTarget.AllBuffered);
            }

        }

    }


    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            // local player
            moveH = Input.GetAxis("Horizontal");
            if (PodeMover)
            {
                _rb.velocity = Movement;
            }
            photonView.RPC("Flip", RpcTarget.AllBuffered);



        }
        else
        {
            // network player
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

            //_namePlayer.text = _nickname;
        }


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
    private void Animations()
    {
        _anim.SetFloat("Velocity", Math.Abs(moveH));
       if (Input.GetKeyDown(KeyCode.Z))
        {
            if (PodeMover)
            {
                Debug.Log("PodeMover");
                _anim.SetTrigger("WeakA");
                attack.damage = 1;
                attack.hitStunDuration = 0.25f;
            }
                
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (PodeMover)
            {
                _anim.SetTrigger("StrongA");
                attack.damage = 1;
                attack.hitStunDuration = 0.5f;
            }
        }

        //if (isHit == true)
        //{
        //    _anim.SetTrigger("Damaged");
        //}
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
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        _anim.SetTrigger("Damaged");

        //_rb.velocity = new Vector2(hitDirection.x * knockbackForce, _rb.velocity.y);

        StartCoroutine(HitCoroutine(hitStunDuration));
    }

    [PunRPC]
    public void Die()
    {
        invincible = true;
        PodeMover = false;
        _anim.SetTrigger("Defeat");
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

}
