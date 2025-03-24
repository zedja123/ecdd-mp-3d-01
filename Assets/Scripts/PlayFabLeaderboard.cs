using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabLeaderboard : MonoBehaviour
{
    public Transform _LBTransform;
    public GameObject _LBRow;
    public GameObject[] _LBEntries;
    public GameObject pf;

    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
        pf = GameObject.Find("Playfab");
    }
    public void RecuperarLeaderboard()
    {
        GetLeaderboardRequest request = new GetLeaderboardRequest
        {
            StartPosition = 0,
            StatisticName = "Kills",
            MaxResultsCount = 10
        };

        PlayFabClientAPI.GetLeaderboard(
            request,
            result =>
            {
                // Clean up the old leaderboard entries (destroy the existing rows)
                if (_LBEntries != null)
                {
                    foreach (GameObject entry in _LBEntries)
                    {
                        Destroy(entry); // Destroy the previous leaderboard entry
                    }
                }

                // TODO: limpar a tabela antes de fazer a rotinha de mostrar os novos resultados

                // fazer um laço para destruir os registros, SE HOUVER registros

                // limpar a lista/array _LBEntries


                // inicializar o array de linhas da tabela
                _LBEntries = new GameObject[result.Leaderboard.Count];

                // popula as linhas da tabela com as informações do playfab
                for (int i = 0; i < _LBEntries.Length; i++)
                {
                    _LBEntries[i] = Instantiate(_LBRow, _LBTransform);
                    TMP_Text[] colunas = _LBEntries[i].GetComponentsInChildren<TMP_Text>();
                    colunas[0].text = result.Leaderboard[i].Position.ToString(); // valor da posição do ranking
                    colunas[1].text = result.Leaderboard[i].DisplayName; // nome do player ou player id
                    colunas[2].text = result.Leaderboard[i].StatValue.ToString(); // valor do estatística
                }
            },
            error => 
            {
                Debug.LogError($"[PlayFab] {error.GenerateErrorReport()}");
            }
        );
    }

    public void UpdateLeaderboard()
    {
        UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "Kills",
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(
            request, 
            result =>
            {
                Debug.Log("[Playfab] Leaderboard foi atualizado!");
            },
            error => 
            {
                Debug.LogError($"[PlayFab] {error.GenerateErrorReport()}");
            }
        );
    }

    public void ShowLeaderboard()
    {

    }
}
