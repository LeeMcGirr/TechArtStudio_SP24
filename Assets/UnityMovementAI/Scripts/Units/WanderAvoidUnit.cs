using UnityEngine;

namespace UnityMovementAI
{
    public class WanderAvoidUnit : MonoBehaviour
    {
        SteeringBasics steeringBasics;
        Wander2 wander;
        CollisionAvoidance colAvoid;

        NearSensor colAvoidSensor;

        public bool yWander;
        public float yWobble;

        void Start()
        {
            steeringBasics = GetComponent<SteeringBasics>();
            wander = GetComponent<Wander2>();
            colAvoid = GetComponent<CollisionAvoidance>();

            colAvoidSensor = transform.Find("ColAvoidSensor").GetComponent<NearSensor>();
        }

        void FixedUpdate()
        {

            Vector3 accel = colAvoid.GetSteering(colAvoidSensor.targets);
            if (yWander)
            {
                accel.y += Random.Range(-yWobble, yWobble);
            }

            if (accel.magnitude < 0.005f)
            {
                accel = wander.GetSteering();
            }

            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();
        }
    }
}