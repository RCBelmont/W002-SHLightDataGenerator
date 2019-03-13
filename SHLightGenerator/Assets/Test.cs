using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public ParticleSystem Ps;
    [Range(0, 2)]
    public float speed = 1;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        var main = Ps.main;
        main.simulationSpeed = speed;
    }
}
