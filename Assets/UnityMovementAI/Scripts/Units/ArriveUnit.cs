using UnityEngine;

namespace UnityMovementAI
{
    public class ArriveUnit : MonoBehaviour
    {

        public Vector3 targetPosition;
        public bool wanderer = false;
        public float wanderRadius = 10f;

        SteeringBasics steeringBasics;

        void Start()
        {
            steeringBasics = GetComponent<SteeringBasics>();
        }

        void FixedUpdate()
        {
            Vector3 accel = steeringBasics.Arrive(targetPosition);

            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();

            if (wanderer & Vector3.Distance(transform.position, targetPosition) < .5f)
            { targetPosition = Random.insideUnitSphere * Random.Range(0f,wanderRadius); }
        }
    }
}