using System.Collections;
using System.Collections.Generic;
using Unity.Android.Types;
using UnityEngine;
using UnityEngine.VFX;

public class helloVFX : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject effectHolder;
    VisualEffect myEffect;
    void Start()
    {
        myEffect = effectHolder.GetComponent<VisualEffect>();
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
    }
}
