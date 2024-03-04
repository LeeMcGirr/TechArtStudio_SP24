using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlekGames.HoverCraftSystem.Systems.Main;


namespace AlekGames.HoverCraftSystem.Systems.Addons
{
    public class HCZoneInput : MonoBehaviour
    {
        private enum whatI { x, y };

        [SerializeField] private whatI whichInput;
        [SerializeField] private hoverCraft vehicle;
        [SerializeField] private string steerTag = "Hand";
        [SerializeField] private Transform center;
        [SerializeField] private Transform front;
        [SerializeField] private Transform end;

        private float fullDistance = 0;
        private void Start()
        {
            fullDistance = Vector3.Distance(front.position, end.position);

        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.CompareTag(steerTag))
            {
                float distanceCenter = Vector3.Distance(other.transform.position, center.position);
                float distanceEnd = Vector3.Distance(other.transform.position, end.position);
                float distanceFront = Vector3.Distance(other.transform.position, front.position);

                float value = distanceCenter;

                if (distanceEnd < distanceFront) value = -distanceCenter;

                value /= (fullDistance / 2);

                Debug.Log(value);

                switch (whichInput)
                {
                    case whatI.x:
                        vehicle.changeXMoveInput(value);
                        break;
                    case whatI.y:
                        vehicle.changeYMoveInput(value);
                        break;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag(steerTag))
            {
                switch (whichInput)
                {
                    case whatI.x:
                        vehicle.changeXMoveInput(0);
                        break;
                    case whatI.y:
                        vehicle.changeYMoveInput(0);
                        break;
                }
            }
        }

    }
}
