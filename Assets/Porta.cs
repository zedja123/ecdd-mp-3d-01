using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Porta : MonoBehaviourPun
{
    private Animator _animator;
    private PhotonView _photonView;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _photonView = GetComponent<PhotonView>();
    }

    public void AbrePorta()
    {
        _photonView.RPC(
            "TocaAnimacao",
            RpcTarget.All
            );

    }

    [PunRPC]
    public void TocaAnimacao()
    {
        _animator.SetTrigger("abrir");
    }


}
