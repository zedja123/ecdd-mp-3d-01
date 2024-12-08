using System;
//using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Fields

    public static PlayerController Instance;
    public static GameObject LocalPlayerInstance;
    private Rigidbody _rb;
    private TMP_Text _namePlayer;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _playerSpeed = 10f;
    private Vector3 networkPosition;
    private string _nickname;

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
        _rb = GetComponent<Rigidbody>();
        _namePlayer = GetComponentInChildren<TMP_Text>();

        if (photonView.IsMine)
        {
            if (LocalPlayerInstance != null) { LocalPlayerInstance = this.gameObject; }
            _nickname = PhotonNetwork.LocalPlayer.NickName;
            var score = PhotonNetwork.LocalPlayer.CustomProperties["Score"];
            _namePlayer.text = _nickname;
            HabilitaMovimentacao(true);
        }
        else
        {
            _namePlayer.text = _nickname;
        }

    }

    [PunRPC]
    public void UpdateScore(int quantidade)
    {
        int scoreAtual = 0;

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Score"))
        {
            scoreAtual = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
        }

        scoreAtual += quantidade;

        // Atualizar essa pontuação nas propriedades customizadas do jogador
        var tabela = new ExitGames.Client.Photon.Hashtable();
        tabela.TryAdd("Score", scoreAtual);
        PhotonNetwork.LocalPlayer.SetCustomProperties(tabela);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log((int)PhotonNetwork.LocalPlayer.CustomProperties["Score"]);
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
            if (PodeMover)
            {
                _rb.velocity = Movement;
            }
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
}
