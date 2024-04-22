using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class ChromAbController : MonoBehaviour
{
    public GameObject myPlayer;
    public float topSpeed = 10f;
    public Volume myVol;
    public VolumeProfile myVolumeProfile;
    public ChromaticAberration myAbb;
    ClampedFloatParameter myIntensity;
    Rigidbody myRB;

    void Start()
    {
        myVolumeProfile = myVol.profile;
        // get the vignette effect
        for (int i = 0; i < myVolumeProfile.components.Count; i++)
        {
            if (myVolumeProfile.components[i].name == "ChromaticAberration(Clone)")
            {
                myAbb = (ChromaticAberration)myVolumeProfile.components[i];
            }
            Debug.Log("vol name: " + myVolumeProfile.components[i].name);
        }

        // get the intensity parameter and all other parameters to be used. 
        // PP parameters are usually an instance of ClampedParameter
        // don't create a new ClampedFloatParameter, get the reference to the existing one
        myIntensity = myAbb.intensity;
        myIntensity.value = 0f;

        myRB = myPlayer.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (myAbb != null)
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
