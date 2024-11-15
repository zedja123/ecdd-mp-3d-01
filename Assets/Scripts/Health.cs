using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Health : MonoBehaviour
{
    
    public int health;
    public bool isLocalPlayer;
    public TextMeshProUGUI healthText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        health -= damage;
        healthText.text = health.ToString();
        Debug.Log("Hit");
        if(health <= 0)
        {
            if (isLocalPlayer)
            {
                GameManager.Instance.Respawn();
            }
            Destroy(gameObject);
        }
    }
}
