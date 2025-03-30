using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleCoins : MonoBehaviour
{
    // Reference to TextMeshProUGUI component in the UI
    public TMP_Text currencyText;

    void Start()
    {
        // Make sure we have a reference to the TMP_Text component
        if (currencyText == null)
        {
            Debug.LogError("Currency Text not assigned in the inspector!");
            return;
        }

        // Get the player's inventory to retrieve the virtual currency
        GetPlayerInventory();
    }

    void GetPlayerInventory()
    {
        // Request the player's inventory from PlayFab
        PlayFabClientAPI.GetUserInventory(
            new GetUserInventoryRequest(),
            result =>
            {
                // Loop through the player's inventory to find the currency (BC)
                foreach (var currency in result.VirtualCurrency)
                {
                    if (currency.Key == "BC") // Replace "BC" with your virtual currency code
                    {
                        // Display the currency amount using TextMeshPro
                        currencyText.text = $"Battle Coins: {currency.Value}";
                        break; // Exit the loop once we find "BC"
                    }
                }
            },
            error =>
            {
                // Handle any errors that occur during the request
                Debug.LogError($"[PlayFab] Error fetching inventory: {error.GenerateErrorReport()}");
            }
        );
    }
}