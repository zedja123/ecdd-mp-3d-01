using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Chat : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputText;
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject message;
    private PhotonView _photonView;

    public Queue<GameObject> _filaMensagem = new Queue<GameObject>();
    private int _chatMensagemQtdMax = 5;

    public delegate void BloqueioMovimento(bool move);
    public BloqueioMovimento bloqueioMovimento;

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();
        bloqueioMovimento = PlayerController.Instance.HabilitaMovimentacao;
    }

    public void BloqueiaMovimento(bool estado)
    {
        bloqueioMovimento.Invoke(estado);
    }

    public GameObject CriaMensagem(string texto)
    {
        GameObject mensagem = Instantiate(message, content.transform);
        mensagem.GetComponent<TMP_Text>().text = texto;
        mensagem.GetComponent<RectTransform>().SetAsFirstSibling();
        return mensagem;
    }


    public void EnviaMensagem()
    {
        _photonView.RPC(
            "RecebeMensagem",
            RpcTarget.All,
            PhotonNetwork.LocalPlayer.NickName + ": " + inputText.text
            );

        inputText.text = "";

    }

    [PunRPC]
    public void RecebeMensagem(string mensagemRecebida)
    {
        if (_filaMensagem.Count <= _chatMensagemQtdMax)
        {
            var mensagem = CriaMensagem(mensagemRecebida);
            _filaMensagem.Enqueue(mensagem);
        }
        else
        {
            var tempMsg = _filaMensagem.Dequeue();
            Destroy(tempMsg);
            var mensagem = CriaMensagem(mensagemRecebida);
            _filaMensagem.Enqueue(mensagem);
        }

    }
}
