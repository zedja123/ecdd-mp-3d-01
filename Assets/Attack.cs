using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Attack : MonoBehaviour
{

    public int damage;
    public float hitStunDuration;
    [SerializeField] public GameObject player;
    void OnTriggerEnter2D(Collider2D other)
    {
        PhotonView playerPhotonView = GetComponentInParent<PhotonView>();
        if (!playerPhotonView.IsMine) return;
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                PlayerController playerController = transform.parent.gameObject.GetComponent<PlayerController>();
                Vector2 hitDirection = (transform.position - other.transform.position).normalized;
                //player.TakeDamage(damage, hitDirection, hitStunDuration); // Sends damage and direction
                player.photonView.RPC("TakeDamage", player.photonView.Owner, damage, hitDirection, hitStunDuration, playerController.playerIsGrounded, playerController.playerIsCrouching);

            }
        }
    }
}
