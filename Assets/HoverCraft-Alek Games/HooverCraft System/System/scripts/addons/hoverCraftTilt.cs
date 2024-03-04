using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlekGames.HoverCraftSystem.Systems.Main;

namespace AlekGames.HoverCraftSystem.Systems.Addons
{
    public class hoverCraftTilt : MonoBehaviour
    {
        private enum tiltUpT { groundNormal, worldUp }

        private enum inputT { thrust, input};

        [SerializeField, Tooltip("specifies what will be counted as Up in tilting")]
        private tiltUpT tiltUpType;

        [SerializeField, Tooltip("if the script should get the player input, or the thrust of hovercraft to tilt")]
        private inputT inputType;

        [SerializeField, Tooltip("the model itself, not the hole hovercraft, make sure that the hoverPoints are not children of it")]
        private Transform model;

        [SerializeField, Tooltip("the hoverCraft component of this hovercraft")]
        private hoverCraft hoverCraftScript;

        [SerializeField, Tooltip("max tilt angle")]
        private float maxTilt = 30;

        [SerializeField, Tooltip("speed of tilt")]
        private float tiltSpeed = 8;


        void Update()
        {
            Vector2 input = inputType == inputT.thrust? hoverCraftScript.getThrustDir(): hoverCraftScript.getCorrectedInput();

            //Debug.Log(input);

            Quaternion startRot = model.rotation;

            Quaternion des = Quaternion.AngleAxis(-input.x * maxTilt, model.forward);

            Quaternion final;

            if (tiltUpType == tiltUpT.groundNormal)
            {
                Vector3 normal = hoverCraftScript.getGroundNormal();
                Vector3 rotatedNormal = des * normal;
                final = Quaternion.LookRotation(model.forward, rotatedNormal);
            }
            else
            {
                Vector3 rotatedUp = des * Vector3.up;
                final = Quaternion.LookRotation(model.forward, rotatedUp);
            }

            model.rotation = Quaternion.Slerp(startRot, final, tiltSpeed * Time.deltaTime);
        }

        public void setup(hoverCraft veicle, Transform model)
        {
            hoverCraftScript = veicle;
            this.model = model;
        }
    }
}
