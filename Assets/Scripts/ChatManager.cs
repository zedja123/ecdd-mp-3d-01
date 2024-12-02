using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _textoMensagem;
    [SerializeField] private GameObject _conteudo;
    [SerializeField] private GameObject _mensagem;
    private PhotonView _photonView;
    

    // Start is called before the first frame update
    void Start()
    {
        _photonView = GetComponent<PhotonView>();        
    }

    public void EnviaMensagem()
    {
        _photonView.RPC(
            "RecebeMensagem",
            RpcTarget.All,
            PhotonNetwork.LocalPlayer.NickName + ": " + _textoMensagem.text
        );

        _textoMensagem.text = "";
    }

    [PunRPC]
    public void RecebeMensagem(string mensagemRecebida)
    {
        GameObject mensagem = Instantiate(_mensagem, _conteudo.transform);

        mensagem.GetComponent<TMP_Text>().text = mensagemRecebida;

        // ultimas mensagens virão primeiro
        mensagem.GetComponent<RectTransform>().SetAsFirstSibling();
    }

}
