using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Health : MonoBehaviour
{
    
    public int health;

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
        health += damage;
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }
}