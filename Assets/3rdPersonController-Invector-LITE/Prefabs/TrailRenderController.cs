using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailRenderController : MonoBehaviour
{
    public Transform[] targets;
    public GameObject[] trails;
    public GameObject trailPrefab;

    Animator playerAnimator;
    string clipName;
    AnimatorClipInfo[] currentClipInfo;

    Vector3 velocity;
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
            foreach(GameObject t in trails)
            {
                t.SetActive(true);
            }
        }
        else
        {
            foreach(GameObject t in trails)
            {
                t.SetActive(false);
            }
        }

        for (int i = 0; i < trails.Length; i++)
        {
            trails[i].transform.position = Vector3.SmoothDamp(trails[i].transform.position, 
                                                                targets[i].transform.position, 
                                                                ref velocity, 
                                                                .1f);
        }
    }
}
