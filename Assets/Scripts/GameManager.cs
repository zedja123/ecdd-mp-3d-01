using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{

    public static GameManager Instance;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform spawnPoint1;
    [SerializeField] Transform spawnPoint2;
    [SerializeField] SpriteRenderer background;
    [SerializeField] Texture2D floresta;
    [SerializeField] Texture2D vila;
    [SerializeField] Texture2D castelo;


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
        playerPrefab = PlayFabLogin.PFL.selectedChar;

        photonView.RPC("ApplyRandomBackground", RpcTarget.All);
        
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

    public void GameOver()
    {
        StartCoroutine(LeaveRoomAndLoadScene(10));
    }

    private IEnumerator LeaveRoomAndLoadScene(float wait)
    {
        yield return new WaitForSeconds(wait);
        if (PhotonNetwork.NetworkClientState != ClientState.Leaving)
        {
            photonView.RPC("LeaveRoom", RpcTarget.All);
        }
        else
        {
            Debug.LogWarning("Client is already leaving the room.");
        }
    }

    [PunRPC]
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("CreateGame");

        base.OnLeftRoom();
    }

    [PunRPC]
    private void ApplyRandomBackground()
    {
        Texture2D selectedTexture = GetRandomTexture();
        background.sprite = TextureToSprite(selectedTexture);
    }

    private Texture2D GetRandomTexture()
    {
        Texture2D[] textures = { floresta, vila, castelo };
        return textures[Random.Range(0, textures.Length)];
    }

    private Sprite TextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
