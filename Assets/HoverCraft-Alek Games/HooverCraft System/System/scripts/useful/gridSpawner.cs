using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlekGames.HoverCraftSystem.Systems.Main;

namespace AlekGames.HoverCraftSystem.Systems.Addons
{
    public class gridSpawner : MonoBehaviour
    {

        public enum shapeT { square, circle };

        [Tooltip("reference to hoverCraft component. if filled in, will automatically assighn created gridPoints as hoverPoints")]
        public hoverCraft hoverCraftScript;

        [Tooltip("if not null will spawn this object as each gridPoint")]
        public GameObject pointObj;

        [Tooltip("shape of grid")]
        public shapeT shape;

        //squre

        [Range(1, 40), Tooltip("spawn iterations on axis x (ammount of grid points spawned on axis x)")]
        public int xSpawn = 5;
        [Range(1, 40), Tooltip("spawn iterations on axis z (ammount of grid points spawned on axis z)")]
        public int zSpawn = 5;

        [Range(0.001f, 3f),Tooltip("distance between grid points on axis x")]
        public float xLen = 0.6f;
        [Range(0.001f, 3f), Tooltip("distance between grid points on axis z")]
        public float zLen = 0.6f;

        //circle

        [Range(0.001f, 3f), Tooltip("radius of each circle layer. going to be multiplied by layer, to make outer circeles further from center")]
        public float radius = 0.6f;
        [Range(-1, 1), Tooltip("up offset on each layer, from layer before")]
        public float up = 0;
        [Range(3, 40), Tooltip("spawned gridPoints on each layer")]
        public int borderSpawn = 8;
        [Range(1, 30), Tooltip("ammount of circles (layers) taht on there are spawned grid points")]
        public int layersCount = 5;

        [ContextMenu("spawn grid")]
        public void spawnGrid()
        {

            Transform point = null;
            if (shape == shapeT.square) point = squereGrid()[0];
            else point = circleGrid()[0];


            if (hoverCraftScript != null) hoverCraftScript.set0HoverPoint(point);
        }


        private Transform[] squereGrid()
        {
            Vector3 moveOffset = new Vector3(-((xSpawn - 1) * xLen) / 2, 0, -((zSpawn - 1) * zLen) / 2);

            int fullCount = xSpawn * zSpawn;
            int curXRow = 0;
            int curZRow = 0;

            Transform[] points = new Transform[fullCount];

            for (int i = 0; i < fullCount; i++)
            {
                points[i] = spawn(i, new Vector3(curXRow * xLen, 0, curZRow * zLen) + moveOffset);

                curXRow++;

                if (curXRow == xSpawn)
                {
                    curXRow = 0;
                    curZRow++;
                }
            }

            return points;
        }

        private Transform[] circleGrid()
        {
            Vector3 dir = transform.forward * radius;

            float degreeTurn = 360f / borderSpawn;

            int curTurn = 0;

            List<Transform> points = new List<Transform>();

            for (int layer = 0; layer < layersCount; layer++)
            {
                while (curTurn < borderSpawn)
                {
                    Transform point = spawn(points.Count, Quaternion.AngleAxis(curTurn * degreeTurn, transform.up) * (dir * (layer + 1)) + Vector3.up * up * layer);
                    points.Add(point);

                    curTurn++;
                }

                curTurn = 0;
            }

            return points.ToArray();
        }



        private Transform spawn(int index, Vector3 posAdd)
        {
            Transform spawned = null;

            if (pointObj != null)
            {
                spawned = Instantiate(pointObj, transform.position + posAdd, transform.rotation).transform;
                spawned.name = "point (" + pointObj.name + "): " + index;
            }
            else
            {
                spawned = new GameObject("point: " + index).transform;
                spawned.position = transform.position + posAdd;
                spawned.rotation = transform.rotation;
            }

            spawned.parent = transform;

            return spawned;
        }


#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;

            switch (shape)
            {
                case shapeT.square:
                    Vector3 moveOffset = new Vector3(-((xSpawn - 1) * xLen) / 2, 0, -((zSpawn - 1) * zLen) / 2);

                    int fullCount = xSpawn * zSpawn;
                    int curXRow = 0;
                    int curZRow = 0;

                    for (int i = 0; i < fullCount; i++)
                    {
                        Gizmos.DrawSphere(transform.position + new Vector3(curXRow * xLen, 0, curZRow * zLen) + moveOffset, 0.05f);

                        curXRow++;

                        if (curXRow == xSpawn)
                        {
                            curXRow = 0;
                            curZRow++;
                        }
                    }

                    break;

                case shapeT.circle:

                    Vector3 dir = transform.forward * radius;

                    float degreeTurn = 360f / borderSpawn;

                    int curTurn = 0;

                    for (int layer = 0; layer < layersCount; layer++)
                    {
                        while (curTurn < borderSpawn)
                        {
                            Vector3 pos = Quaternion.AngleAxis(curTurn * degreeTurn, transform.up) * (dir * (layer + 1)) + Vector3.up * up * layer;

                            Gizmos.DrawLine(transform.position, transform.position + pos);
                            //Debug.Log(layer + "  " + curTurn);

                            Gizmos.DrawSphere(transform.position + pos, 0.05f);

                            curTurn++;
                        }

                        curTurn = 0;
                    }

                    break;
            }

        }

#endif
    }

}
