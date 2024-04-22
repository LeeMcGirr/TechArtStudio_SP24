using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class abberationControl : MonoBehaviour
{

    public Volume myVol;
    public VolumeProfile myVolumeProfile;
    public ChromaticAberration myAbb;
    ClampedFloatParameter myIntensity;

    public GameObject myPlayer;
    public float topSpeed = 2f;
    Rigidbody myRB;

    // Start is called before the first frame update
    void Start()
    {
        myVolumeProfile = myVol.profile;
        for (int i = 0; i < myVolumeProfile.components.Count; i++)
        {
            Debug.Log("vol name: " + myVolumeProfile.components[i].name);
            if (myVolumeProfile.components[i].name == "ChromaticAberration(Clone)")
            {
                myAbb = (ChromaticAberration)myVolumeProfile.components[i];
            }
        }

        myIntensity = myAbb.intensity;
        myIntensity.value = 0f;

        myRB = myPlayer.GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (myAbb !=null)
        {
            float newIntensity = Mathf.Clamp(myRB.velocity.magnitude, 0f, topSpeed);
            newIntensity = RemapFloat(newIntensity, 0f, topSpeed, 0f, 1f);
            myIntensity.value = newIntensity;
        }
    }

    public static float RemapFloat(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
