using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlekGames.HoverCraftSystem.Systems.Main;

namespace AlekGames.HoverCraftSystem.Systems.Addons
{

    public class HCTiresSimulator : MonoBehaviour
    {
        public enum upT {localUp, worldUp, normal};

        [SerializeField, Tooltip("the hoverCraft component of this hovercraft")]
        private hoverCraft hoverCraftScript;
        [SerializeField, Tooltip("forward of hoverCraft. this represents up of it too, so point up of it, to up of hovercraft")] 
        private Transform forwardAxis;
        [SerializeField, Tooltip("max distance of tire to detectPoint")] 
        private float detectionHeight = 1.8f;
        [SerializeField, Tooltip("offset of tires from groun (their radius)")] 
        private float tireHeigthOffset = 0.33f;
        [SerializeField, Tooltip("turn angle of turning tires")]
        private float turnAngle;
        [SerializeField, Tooltip("the speed of turning of turning tires")]
        private float turnSpeed;
        [SerializeField, Tooltip("ground layer")] 
        private LayerMask groundLayer;
        [SerializeField, Tooltip("specifies what should be considered as up for a tire. up - V3.up. normal - normal of detected surface")] 
        private upT upType;

        [SerializeField, Tooltip("lis of tires that will turn")] 
        tireSettings[] turningTires;
        [SerializeField, Tooltip("list of tires that just adapt to terrain")] 
        tireSettings[] standingTires;

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < turningTires.Length; i++)
            {
                float turn = hoverCraftScript.getInput().x;
                Vector3 up = positionTire(turningTires[i].tire, turningTires[i].detectPoint);

                Vector3 rotatedForward = Quaternion.AngleAxis(turn * turnAngle, forwardAxis.up) * forwardAxis.forward;

                Quaternion final = Quaternion.LookRotation(rotatedForward , up);

                turningTires[i].tire.rotation = Quaternion.Slerp(turningTires[i].tire.rotation, final, 360 / turnSpeed * Time.deltaTime);
            }

            for (int i = 0; i < standingTires.Length; i++)
            {
                standingTires[i].tire.rotation = Quaternion.LookRotation(forwardAxis.forward, positionTire(standingTires[i].tire, standingTires[i].detectPoint));
            }
           
            
        }

        private Vector3 positionTire(Transform tire, Transform hoverPoint)
        {
            if (Physics.Raycast(hoverPoint.position, -hoverPoint.up, out RaycastHit info, detectionHeight, groundLayer))
            {
                Vector3 up = Vector3.up;
                if (upType == upT.localUp) up = (hoverPoint.position - info.point);
                else if (upType == upT.normal) up = info.normal;

                tire.position = info.point + up * tireHeigthOffset;

                return up;
            }
            else
            {
                tire.position = hoverPoint.position + (-hoverPoint.up * (detectionHeight - tireHeigthOffset));

                return Vector3.up;
            }
        }

        [System.Serializable]
        public struct tireSettings
        {
            [Tooltip("point of with tire will be palced on end of ray shoot down from")]
            public Transform detectPoint;
            [Tooltip("tire itself")]
            public Transform tire;
        }
    }
}
