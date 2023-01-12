using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fireEffect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Destroy", 1.1f);
    }

    void Destroy()
    {
        Destroy(gameObject);
    }
}
