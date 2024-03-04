using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlekGames.HoverCraftSystem.Systems.Main
{
    public class camera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float speed;
        [SerializeField] private Transform cam;
        [SerializeField] private Transform camPos;
        // Start is called before the first frame update
        void Start()
        {
            if (cam == null) cam = Camera.main.transform;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Vector3 configuratedCamPos = new Vector3(camPos.position.x, camPos.position.y, camPos.position.z);
            float distance = Vector3.Distance(camPos.position, cam.position);
            cam.position = Vector3.MoveTowards(cam.position, configuratedCamPos, speed * Time.deltaTime * distance);

            cam.LookAt(target);

        }

        public void setup(Transform target, Transform cam, Transform camPos)
        {
            this.target = target;
            this.cam = cam;
            this.camPos = camPos;
            speed = 15;
        }
    }
}
