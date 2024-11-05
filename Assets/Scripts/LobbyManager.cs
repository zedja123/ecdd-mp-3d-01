using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1.0.0";

    private void Awake()
    {
        // # Crítico
        // isso garante que possamos usar PhotonNetwork.LoadLevel()
        // no cliente mestre e todos os clientes na mesma sala
        // sincronizarão seus níveis automaticamente
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        Conetar();
    }

    private void Conetar()
    {
        if (PhotonNetwork.IsConnected) 
        {
            PhotonNetwork.JoinRandomRoom();
        } else
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado ao Photon Master Server");
        // Entra em uma sala aleatória
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Falha ao entrar em uma sala aleatória, criando uma nova sala");
        // Cria uma nova sala
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Entrou na sala com sucesso");
        Debug.Log("Id: " + PhotonNetwork.CurrentRoom.Name);
        
        // Carrega a cena do jogo para todos na sala
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }
}
