using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class helperScript : MonoBehaviour
{
    public GameObject myLiquid;
    float time;
    // Start is called before the first frame update
    void Start()
    {
        myLiquid.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(time > 0) { myLiquid.SetActive(true); }
        time += Time.deltaTime;
    }
}
