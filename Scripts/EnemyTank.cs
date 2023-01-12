using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTank : MonoBehaviour
{

    //public GameObject turret;
    //public GameObject body;
    public GameObject tankWreck;
    public Transform player;
    Vector3 laserDirection;

    GameObject playerT;
    string hitName;
    float timer = 0;
    public GameObject hitEffect2;
    public Transform cannon;

    public GameObject hitEffect3;

    //get player object
    public GameObject playerTank;
    public PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        //to desynchronize the fire of enemies
        timer = Random.Range(0.0f, 1.6f);

        playerMovement = playerTank.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        RaycastGun();
    }

    public void LaserHit()
    {
        Instantiate(tankWreck, transform.position, Quaternion.identity);

        ToPlayer();

        Destroy(gameObject);
    }

    void RaycastGun()
    {
        if (timer <= 1.1)
        {
            timer += Time.deltaTime;
        }
        else
        {
            //cannonPos = cannon;
            laserDirection = player.position - cannon.transform.position;
            RaycastHit hit;
            if (Physics.Raycast(cannon.transform.position, laserDirection, out hit, 500))
            {
                //Debug.DrawRay(cannon.transform.position, laserDirection * hit.distance, Color.yellow);
                //Debug.Log(hit.collider.gameObject.name + " shot down");
                hitName = hit.collider.gameObject.name;

                if (hitName == "player")
                {
                    //EnemyHitbox enemyHitbox = hit.collider.gameObject.GetComponent<EnemyHitbox>();
                    playerT = hit.collider.gameObject;
                    PlayerHitbox playerHitbox = playerT.GetComponent<PlayerHitbox>();
                    playerHitbox.LaserHit2();
                    //Debug.Log("tanký vuruyom");

                    Instantiate(hitEffect3, cannon.transform.position, Quaternion.identity);

                    Instantiate(hitEffect2, hit.point, Quaternion.identity);
                }
                else
                {
                    //Debug.Log("boþa sýkýyom");
                }
            }
            timer = 0;
        }

    }

    private void ToPlayer()
    {
        playerMovement.Healthup();
    } 
}