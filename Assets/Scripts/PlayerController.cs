using System;
//using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Fields
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

    private float moveH;
    private int _localScore;

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
        if (photonView.IsMine)
        {
            if (LocalPlayerInstance != null) { LocalPlayerInstance = this.gameObject; }
            isFacingRight = true;
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
    }


    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            // local player
            moveH = Input.GetAxis("Horizontal");
            _rb.velocity = Movement;
            MovementAnimations();
            Flip();


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

    private void MovementAnimations()
    {
        _anim.SetFloat("Velocity", Math.Abs(moveH));
    }
}
