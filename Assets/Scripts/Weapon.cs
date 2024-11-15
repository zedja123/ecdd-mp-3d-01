using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UIElements;

public class Weapon : MonoBehaviour
{

    public GameObject hitVFX;

    public Camera cam;

    public int damage;

    public float fireRate;

    private float nextFire;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (nextFire > 0)
        {
            nextFire -= Time.deltaTime;
        }
        if (Input.GetButton("Fire1") && nextFire <= 0)
        {
            Debug.Log("Firing");
            nextFire = 1 / fireRate;

            Fire();
        }
    }
    [PunRPC]
    void Fire()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000f))
        {
            PhotonNetwork.Instantiate(hitVFX.name, hit.point, Quaternion.identity);

            if (hit.transform.gameObject.GetComponent<Health>())
            {
                hit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage);
                Debug.DrawLine(ray.origin, hit.point);
            }
        }
    }
}
