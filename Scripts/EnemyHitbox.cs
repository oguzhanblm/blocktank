using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public GameObject parentt;
    public EnemyTank enemyTank;

    // Start is called before the first frame update
    void Start()
    {
        enemyTank = parentt.GetComponent<EnemyTank>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LaserHit2()
    {
        enemyTank.LaserHit();
    }
}