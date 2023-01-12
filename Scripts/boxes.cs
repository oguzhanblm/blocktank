using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boxes : MonoBehaviour
{
    public GameObject tank;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider go)
    {
        //Debug.Log("anan");
        tank.transform.Translate(Vector3.up * 10 * Time.deltaTime, Space.World);

    }
}
