using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{

    [SerializeField] float moveSpeed = 10f;


    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            float moveH = Input.GetAxis("Horizontal");
            float moveV = Input.GetAxis("Vertical");

            Vector3 move = new Vector3(moveH, 0.0f, moveV);

            transform.Translate( move * moveSpeed * Time.deltaTime);
        }
    }
}
