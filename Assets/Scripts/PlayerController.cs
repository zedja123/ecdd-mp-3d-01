using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Fields

    public static GameObject Instance;
    public static GameObject LocalPlayerInstance;
    private Rigidbody _rb;
    private TMP_Text _namePlayer;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _playerSpeed = 10f;
    private Vector3 networkPosition;
    private string _nickname;

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

    private void Awake()
    {
        //if (Instance == null)
        //{
        //    Instance = this.gameObject;
        //}
    }

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _namePlayer = GetComponentInChildren<TMP_Text>();

        if (photonView.IsMine)
        {
            if (LocalPlayerInstance != null) { LocalPlayerInstance = this.gameObject; }
            _nickname = PhotonNetwork.LocalPlayer.NickName;
            _namePlayer.text = _nickname;
        }
        else
        {
            _namePlayer.text = _nickname;
        }

    }

    // Update is called once per frame
    void Update()
    {
        float moveH = Input.GetAxis("Horizontal");
        float moveV = Input.GetAxis("Vertical");
        bool isJumpPressed = Input.GetButtonDown("Jump");
        float jump = isJumpPressed ? _rb.velocity.y + JumpForce : _rb.velocity.y;
        Movement = new Vector3(moveH * PlayerSpeed, jump, moveV * PlayerSpeed);
    }


    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            // local player
            _rb.velocity = Movement;
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

            if (photonView.IsMine)
            {
                _namePlayer.text = PhotonNetwork.LocalPlayer.NickName;
            }
            else
            {
                _namePlayer.text = _nickname;
            }
        }


    }
}
