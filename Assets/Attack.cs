using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Attack : MonoBehaviour
{

    public int damage;
    public float hitStunDuration;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Vector2 hitDirection = (transform.position - other.transform.position).normalized;

                //player.TakeDamage(damage, hitDirection, hitStunDuration); // Sends damage and direction
                player.photonView.RPC("TakeDamage", RpcTarget.All, damage, hitDirection, hitStunDuration);
            }
        }
    }
}
