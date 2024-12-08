using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Coletaveis : MonoBehaviourPun
{
    [SerializeField] private int quatidade = 1;
    public UnityEvent AoPegar;


    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.GetPhotonView().IsMine)
            return;

        other.GetComponent<PlayerController>().UpdateScore(quatidade);
        
        AoPegar.Invoke();

        photonView.RPC("DestroiItem", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void DestroiItem()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }

}
