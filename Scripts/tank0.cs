using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tank0 : MonoBehaviour
{
    private Rigidbody rb;

    public int ileri;
    public int sag;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        ileri = 100;
        sag = 100;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
            rb.AddRelativeForce(Vector3.forward * ileri);
        if (Input.GetKey(KeyCode.S))
            rb.AddRelativeForce(Vector3.back * ileri);
        if (Input.GetKey(KeyCode.D))
            rb.AddRelativeForce(Vector3.right * sag);
        if (Input.GetKey(KeyCode.A))
            rb.AddRelativeForce(Vector3.left * sag);
    }
}
