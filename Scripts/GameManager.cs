using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{

    public static GameManager Instance;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform playerSpawnerPosition;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }


    // Start is called before the first frame update
    void Start()
    {

        if (PlayerController.LocalPlayerInstance == null)
        {
            PhotonNetwork.Instantiate("Prefabs/" + playerPrefab.name, playerSpawnerPosition.position, Quaternion.identity);
        }

        var _listPlayer = PhotonNetwork.PlayerList;

        foreach (var player in _listPlayer)
        {
            Debug.Log(player.NickName);
        }


    }

    // Update is called once per frame
    void Update()
    {
        //foreach (var player in PhotonNetwork.PlayerList)
        //{
        //    Debug.Log(player.NickName + ": " + player.CustomProperties["Score"]);
        //}
    }
}
