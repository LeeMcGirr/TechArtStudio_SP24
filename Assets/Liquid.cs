using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Liquid : MonoBehaviour
{

    [Header("Base Vars")]
    public float fillAmount = 0.5f;
    Mesh myMesh;
    Renderer myRend;
    Vector3 myPos;

    [Header("Wobble Vars")]
    public float MaxWobble = 0.05f;
    public float WobbleSpeed = 1f;
    public float wobbleRecovery = 1f;
    public float wobbleThickness = 1f;
    public float lowPosCompensation = .2f;

    //sinewave and actual wobble add vars
    Vector2 wobbleAmount;
    Vector2 wobbleToAdd;
    float pulse;
    float sinewave;
    float time = 0.5f;

    //pos and vel vars
    Vector3 velocity;
    Vector3 angularVelocity;
    Vector3 lastPos;
    Quaternion lastRot;

    // Start is called before the first frame update
    void Start()
    {
        myMesh = GetComponent<MeshFilter>().sharedMesh;
        myRend = GetComponent<Renderer>();
        wobbleToAdd = Vector2.zero;
        wobbleAmount = Vector2.zero;
    }

    //OnValidate runs during Editor runtime, not just play time
    //we're using it here in conjunction with [ExecuteInEditMode] for live water update viz
    private void OnValidate()
    {
        myMesh = GetComponent<MeshFilter>().sharedMesh;
        myRend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime;
        time += deltaTime;

        if(deltaTime != 0)
        {
            //decrease wobble over time when not in motion
            wobbleToAdd = Vector2.Lerp(wobbleToAdd, Vector2.zero, (deltaTime * wobbleRecovery));

            //apply a sine wave based on time to wobble
            pulse = 2 * Mathf.PI * WobbleSpeed;
            sinewave = Mathf.Lerp(sinewave, Mathf.Sin(pulse * time), deltaTime * Mathf.Clamp(velocity.magnitude + angularVelocity.magnitude, wobbleThickness, 10));

            wobbleAmount = wobbleToAdd * sinewave;

            //get our velocity and our angular (rotational) velocity
            velocity = (lastPos - transform.position) / deltaTime;
            angularVelocity = GetAngularVelocity(lastRot, transform.rotation);

            //here we're finding the vel to add along the X/Z construction plane
            //we take a small amount of Y vel but mostly X/Z vels, then add our angular velocities and clamp to wobble
            wobbleToAdd.x += Mathf.Clamp((velocity.x + (velocity.y * .2f) + angularVelocity.z + angularVelocity.y) * MaxWobble, -MaxWobble, MaxWobble);
            //the wobble.y here is ACTUALLY FOR THE Z CHANNEL IN SHADER
            wobbleToAdd.y += Mathf.Clamp((velocity.z + (velocity.y * .2f) + angularVelocity.x + angularVelocity.y) * MaxWobble, -MaxWobble, MaxWobble);

            //finally send our wobble velocities to the shader!
            myRend.sharedMaterial.SetFloat("_WobbleX", wobbleAmount.x);
            myRend.sharedMaterial.SetFloat("_WobbleZ", wobbleAmount.y);
        
        }

        //set a base fill amount
        FillPos(deltaTime);

        //store positions for next update cycle
        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    void FillPos(float deltaTime)
    {
        float fillMod = 0f;

        //find the center of the mesh first, then translate to world space
        //we work in world space because the .shader needs world space for the waterline
        Vector3 worldPos = transform.TransformPoint(myMesh.bounds.center);

        //fillMod checks for lowest point and shifts the water line up if there is enough variation in lowest point
        //this stops cylinders from having "disappearing" water, essentially
        fillMod = Mathf.Lerp(fillMod, (worldPos.y - GetLowestPoint()), deltaTime);
        myPos = worldPos - transform.position - new Vector3(0, fillAmount - (fillMod*lowPosCompensation), 0);


        myRend.sharedMaterial.SetVector("_FillAmount", myPos);

    }

    //borrowed from unity thread here
    //https://forum.unity.com/threads/manually-calculate-angular-velocity-of-gameobject.289462/#post-4302796
    Vector3 GetAngularVelocity(Quaternion foreLastFrameRotation, Quaternion lastFrameRotation)
    {
        var q = lastFrameRotation * Quaternion.Inverse(foreLastFrameRotation);
        // no rotation?
        // You may want to increase this closer to 1 if you want to handle very small rotations.
        // Beware, if it is too close to one your answer will be Nan
        if (Mathf.Abs(q.w) > 1023.5f / 1024.0f)
            return Vector3.zero;
        float gain;
        // handle negatives, we could just flip it but this is faster
        if (q.w < 0.0f)
        {
            var angle = Mathf.Acos(-q.w);
            gain = -2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
        }
        else
        {
            var angle = Mathf.Acos(q.w);
            gain = 2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
        }
        Vector3 angular = new Vector3(q.x * gain, q.y * gain, q.z * gain);

        if (float.IsNaN(angularVelocity.z))
        {
            angularVelocity = Vector3.zero;
        }
        return angular;
    }

    //this will tell us how deep the mesh is so we don't overshoot its depth with the wobble
    float GetLowestPoint()
    {
        //set our min and max values to the largest/smallest possible, then declare our vertex array
        float lowY = float.MaxValue;
        Vector3 lowVert = Vector3.zero;
        Vector3[] vertices = myMesh.vertices;

        //this loops through each vertex and replaces our low values whenever it is lower than current
        foreach (var vertex in vertices)
        {
            Vector3 vertPos = transform.TransformPoint(vertex);
            if (vertPos.y < lowY) { lowY = vertPos.y; lowVert = vertPos; }
        }

        //finally return the lowest out of the array
        return lowY;
    }
}
