using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXController : MonoBehaviour
{

    [Header("FootSteps")]
    public ParticleSystem leftFoot, rightFoot;

    [Header("VelocityLines")]
    public Transform[] targets;
    public GameObject[] trails;
    public GameObject trailPrefab;
    Vector3 currentVel;

    [Header("Animator Refs")]
    Animator playerAnimator;
    string clipName;
    AnimatorClipInfo[] currentClipInfo;


    void FootStepEvent(int f)
    {
        Debug.Log("foot stepped: " + f);
        if(f == 0)
        {
            leftFoot.Play();
        }
        else { rightFoot.Play(); }

    }
    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = gameObject.GetComponent<Animator>();


        trails = new GameObject[targets.Length];
        for(int i = 0; i < targets.Length; i++)
        {
            trails[i] = Instantiate(trailPrefab, targets[i].position, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentClipInfo = playerAnimator.GetCurrentAnimatorClipInfo(0);
        clipName = currentClipInfo[0].clip.name;
        Debug.Log(clipName);

        if(clipName == "Run")
        {
            foreach (GameObject t in trails)
            {
                t.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject t in trails)
            {
                t.SetActive(false);
            }
        }

        //Vector3.SmoothDamp(position, target, ref velocity, totalTime)
        for (int i = 0; i < trails.Length; i++)
        {
            trails[i].transform.position = Vector3.SmoothDamp(trails[i].transform.position, targets[i].transform.position, ref currentVel, .1f);
        }
    }
}
