using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepController : MonoBehaviour
{
    public ParticleSystem leftFoot, rightFoot;
    void FootStepEvent(int foot) 
    { 
        Debug.Log("footStepped: " + foot);
        if(foot == 1) 
        {
            leftFoot.Play();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
