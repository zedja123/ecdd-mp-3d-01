using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<GameObject> _playersPanels;
    [SerializeField] private TMP_Text _textPlayerCount;
    [SerializeField] public Image P1Sprite;
    [SerializeField] public Image P2Sprite;
    [SerializeField] public GameObject startBtn;
    int _playersCount;


    private void Awake()
    {
        ChecaJogadores();
    }

    private void Update()
    {
        ChecaJogadores();
    }

    private void ChecaJogadores()
    {
        _playersCount = PhotonNetwork.CurrentRoom.PlayerCount;
        Player[] playersList = PhotonNetwork.PlayerList;

        if (_playersCount <= 0)
            return;

        _textPlayerCount.text = _playersCount.ToString();

        for (int i = 0; i < _playersCount; i++)
        {
            _playersPanels[i].SetActive(true);
            _playersPanels[i].GetComponentInChildren<TMP_Text>().text = playersList[i].NickName;

            if (playersList[i].CustomProperties.ContainsKey("CharName"))
            {
                string charName = (string)playersList[i].CustomProperties["CharName"];
                Sprite charSprite = Resources.Load<Sprite>(charName);

                // Assign sprite to correct image
                if (i == 0)
                    P1Sprite.sprite = charSprite;
                else if (i == 1)
                    P2Sprite.sprite = charSprite;
            }
        }

        if (_playersCount == 2 && PhotonNetwork.IsMasterClient)
        {
            startBtn.SetActive(true);
        }
    }



    public void StartGame()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }
}
