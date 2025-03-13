using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.DataModels;
using PlayFab.ProfilesModels;
using PlayFab.ClientModels;
using TMPro;
using Unity.VisualScripting;
using ExitGames.Client.Photon;

public class PlayFabLogin : MonoBehaviour
{

    public static string PlayFabID;
    public string Nickname;
    public string EntityID;
    public string EntityType;

    public TMP_Text statusText;

    public string usernameOrEmail;
    public string userPassword;
    public string username;

    // campos utilizados para efetuar o login do jogador
    public TMP_InputField inputUserEmailLogin;
    public TMP_InputField inputUserPasswordLogin;

    // campos utilizados para criar uma nova conta para o jogador
    public TMP_InputField inputUsername;
    public TMP_InputField inputEmail;
    public TMP_InputField inputPassword;

    public GameObject loginPanel;

    public CarregamentoEConexao loadManager;

    public static PlayFabLogin PFL;

    private void Awake()
    {
        // singleton
        if (PFL != null && PFL != this)
        {
            Destroy(PFL);
        }
        PFL = this;
        DontDestroyOnLoad(this.gameObject);
    }

    #region Login

    public void Login()
    {
        if (string.IsNullOrEmpty(inputUserEmailLogin.text) || string.IsNullOrEmpty(inputUserPasswordLogin.text))
        {
            Debug.Log("Preencha os dados corretamente!");
            statusText.text = "Preencha os dados corretamente!";
        }
        else
        {
            // credenciais para autentica��o
            usernameOrEmail = inputUserEmailLogin.text;
            userPassword = inputUserPasswordLogin.text;

            if (usernameOrEmail.Contains("@"))
            {
                //payload de requisi��o
                var requestEmail = new LoginWithEmailAddressRequest { Email = usernameOrEmail, Password = userPassword };

                // Requisi��o
                PlayFabClientAPI.LoginWithEmailAddress(requestEmail, SucessoLogin, FalhaLogin);
            }
            else
            {
                //payload de requisi��o
                var requestUsername = new LoginWithPlayFabRequest { Username = usernameOrEmail, Password = userPassword };

                // Requisi��o
                PlayFabClientAPI.LoginWithPlayFab(requestUsername, SucessoLogin, FalhaLogin);

            }
        }
    }

    public void CriarConta()
    {
        if (string.IsNullOrEmpty(inputUsername.text) || string.IsNullOrEmpty(inputEmail.text) || string.IsNullOrEmpty(inputPassword.text))
        {
            Debug.Log("Preencha os dados corretamente!");
            statusText.text = "Preencha os dados corretamente!";
        }
        else
        {
            username = inputUsername.text;
            usernameOrEmail = inputEmail.text;
            userPassword = inputPassword.text;

            // payload da requisi��o
            var request = new RegisterPlayFabUserRequest { Email = usernameOrEmail, Password = userPassword, Username = username };

            // Requisi��o
            PlayFabClientAPI.RegisterPlayFabUser(request, SucessoCriarConta, FalhaCriarConta);
        }
    }

    public void SucessoLogin(LoginResult result)
    {
        // captura o playfabID
        PlayFabID = result.PlayFabId;

        // Mensagens de status
        Debug.Log("Login foi feito com sucesso!");
        statusText.text = "Login foi feito com sucesso!";

        // desabilita o painel de login
        loginPanel.SetActive(false);

        // captura do nickname
        PegaDisplayName(PlayFabID);

        if (result.EntityToken != null && result.EntityToken.Entity != null)
        {
            EntityID = result.EntityToken.Entity.Id;
            EntityType = result.EntityToken.Entity.Type;

            Debug.Log($"EntityID: {EntityID}, EntityType: {EntityType}");
        }
        else
        {
            Debug.LogWarning("O LoginResult n�o retornou EntityToken. Talvez seja preciso chamar GetEntityToken separadamente.");
        }

        // carrega nova cena e conecta no photon
        loadManager.Connect();
    }

    public void FalhaLogin(PlayFabError error)
    {
        // Mensagem de status
        Debug.Log("N�o foi poss�vel fazer login!");
        statusText.text = "N�o foi poss�vel fazer login!";

        // tratamento de erros
        switch (error.Error)
        {
            case PlayFabErrorCode.AccountNotFound:
                statusText.text = "N�o foi poss�vel efetuar o login!\nConta n�o existe.";
                break;
            case PlayFabErrorCode.InvalidEmailOrPassword:
                statusText.text = "N�o foi poss�vel efetuar o login!\nE-mail ou senha inv�lidos.";
                break;
            default:
                statusText.text = "N�o foi poss�vel efetuar o login!\nVerifique os dados infomados.";
                break;

        }
    }

    public void FalhaCriarConta(PlayFabError error)
    {
        // Mensagem de status
        Debug.Log("Falhou a tentativa de criar uma conta nova");
        statusText.text = "Falhou a tentativa de criar uma conta nova";

        // tratamento de erros
        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidEmailAddress:
                statusText.text = "J� possui um conta com esse email!";
                break;
            case PlayFabErrorCode.InvalidUsername:
                statusText.text = "Username j� est� em uso.";
                break;
            case PlayFabErrorCode.InvalidParams:
                statusText.text = "N�o foi poss�vel criar um conta! \nVerifique os dados informados";
                break;
            default:
                statusText.text = "N�o foi poss�vel efetuar o login!\nVerifique os dados infomados.";
                Debug.Log(error.ErrorMessage);
                break;
        }
    }

    public void SucessoCriarConta(RegisterPlayFabUserResult result)
    {
        // Mensagem de status
        Debug.Log("Sucesso ao criar uma conta nova!");
        statusText.text = "Sucesso ao criar uma conta nova!";
    }

    #endregion

    public void PegaDadosJogador(string id)
    {
        // requisi��o para pegar dados do jogador
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = PlayFabID,
            Keys = null
        }, result => {

            if (result.Data == null || !result.Data.ContainsKey(id))
            {
                Debug.Log("Conte�do vazio!");
            }

            else if (result.Data.ContainsKey(id))
            {
                PlayerPrefs.SetString(id, result.Data[id].Value);
            }

        }, (error) => {
            Debug.Log(error.GenerateErrorReport());
        });
    }

    public void SalvaDadosJogador(string id, string valor)
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {
                {id, valor}
            }
        },
        result => Debug.Log("Dados do jogador atualizados com sucesso!"),
        error => {
            Debug.Log(error.GenerateErrorReport());
        });
    }

    public void PegaDisplayName(string playFabId)
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest()
        {
            PlayFabId = playFabId,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true
            }
        },
        result => {
            Nickname = result.PlayerProfile.DisplayName;
        },
        error => Debug.Log(error.ErrorMessage));
    }
}
