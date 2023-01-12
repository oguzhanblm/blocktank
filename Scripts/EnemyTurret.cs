using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurret : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform player;
    float timer = 0;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        TurretRotate();
    }

    void TurretRotate()
    {
        if(timer <= 0.1)
        {
            timer += Time.deltaTime;
        }
        else
        {
            Vector3 relative = transform.InverseTransformPoint(player.position);
            float angle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
            transform.Rotate(0, angle, 0);

            timer = 0;
        }
    }
}
