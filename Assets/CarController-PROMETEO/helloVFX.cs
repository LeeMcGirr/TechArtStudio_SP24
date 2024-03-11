using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class helloVFX : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject effectHolder;
    VisualEffect myEffect;
    Rigidbody myRB;
    void Start()
    {
        myEffect = effectHolder.GetComponent<VisualEffect>();
        myRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (effectHolder.activeInHierarchy == false) { effectHolder.SetActive(true); }
            myEffect.Play();
        }
        myRB.AddExplosionForce(1000f, collision.gameObject.transform.position-Vector3.up, 20f, 30f);
    }
}
