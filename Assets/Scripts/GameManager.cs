using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{

    public static GameManager Instance;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform spawnPoint1;
    [SerializeField] Transform spawnPoint2;


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

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Spawned")) return;
        if (PlayerController.LocalPlayerInstance == null)
        {
            Transform spawnPosition = PhotonNetwork.IsMasterClient ? spawnPoint1 : spawnPoint2;
            GameObject player = PhotonNetwork.Instantiate("Prefabs/" + playerPrefab.name, spawnPosition.position, Quaternion.identity);

            // Mark this player as spawned to prevent duplicates
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Spawned", true } });

            Debug.Log($"[Photon] Instantiated Player: {player.name} | ViewID: {player.GetComponent<PhotonView>().ViewID} | IsMine: {player.GetComponent<PhotonView>().IsMine}");
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
