using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class explosionTrigger : MonoBehaviour
{
    public float explosionForce = 10f;
    public float explosionTorque = 10f;
    public GameObject myExplosion;
    public bool exploded;
    Rigidbody myRB;

    // Start is called before the first frame update
    void Start()
    {
        exploded = false;
        myRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        Vector3 force = (Random.insideUnitSphere + Vector3.up).normalized * explosionForce;
        Vector3 torque = Random.insideUnitSphere * explosionTorque;

        if (!exploded && collision.other.gameObject.tag != "Ground")
        {
            GameObject obj = Instantiate(myExplosion, transform.position, Quaternion.identity);
            obj.GetComponent<VisualEffect>().Play();
            myRB.AddForce(force);
            myRB.AddTorque(torque);
            exploded = true;
        }

    }
}
