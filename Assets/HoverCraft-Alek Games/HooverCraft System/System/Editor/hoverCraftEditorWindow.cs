using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using AlekGames.HoverCraftSystem.Systems.Main;
using AlekGames.HoverCraftSystem.Systems.Addons;

#if UNITY_EDITOR


namespace AlekGames.Editor
{
    public class hoverCraftEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Alek Games/HoverCraft System/hoverCraft Maker #&H")]
        public static void showWindow()
        {
            GetWindow<hoverCraftEditorWindow>("hoverCraft Maker");
        }

        Transform model;
        bool input = false;
        bool cameraSetup = true;
        bool tilting = true;
        bool hoverPoints = true;
        LayerMask groundLayer;

        private void OnGUI()
        {
            if (model == null && Selection.transforms.Length > 0) model = Selection.transforms[0];         


            model = EditorGUILayout.ObjectField("model: ", model, typeof(Transform), true) as Transform;

            GUILayout.Space(15);

            GUILayout.Label("weird bug with layer have been fixed, you can select correct layer now (in the tutorial i sand to select one up, now you can select it normaly)", EditorStyles.helpBox);
            groundLayer = EditorGUILayout.LayerField("ground layer: ", groundLayer);


            GUILayout.Space(20);

            input = EditorGUILayout.Toggle("new Input System Wraper: ", input);
            cameraSetup = EditorGUILayout.Toggle("setup camera: ", cameraSetup);
            tilting = EditorGUILayout.Toggle("add tilting: ", tilting);
            hoverPoints = EditorGUILayout.Toggle("add HoverPoints: ", hoverPoints);

            GUILayout.Space(20);

            EditorGUILayout.HelpBox("make sure that the model is rotated so that the model forward (not nessesarly the transform) = V3.forward", MessageType.Info);

            GUILayout.Space(10);

            if (model == null)
            {
                GUILayout.Space(20);
                EditorGUILayout.HelpBox("select model", MessageType.Error, true);
                return;
            }

            if (GUILayout.Button("Generate"))
            {
                //Debug.Log(groundLayer.value);
                groundLayer += 1; //layer nothing is not counted in layer field, but it is counted elewere, so i just pick one above, couse layer nothing pushes all layers by 1
                generate();
                groundLayer -= 1;
            }
        }

        private void generate()
        {
            Transform root = new GameObject(model.name + " Root").transform;
            root.position = model.position;

            if (model.parent != null) root.parent = model.parent;


            Transform veicle = new GameObject("HoverCraft").transform;
            veicle.position = root.position;
            veicle.rotation = root.rotation;
            veicle.parent = root;


            Transform meshHold = new GameObject("meshHold").transform;
            meshHold.position = model.position;
            meshHold.parent = veicle;
            model.parent = meshHold;


            Transform center = new GameObject("center").transform;
            center.parent = veicle;
            center.localPosition = Vector3.zero;


            Transform gd = new GameObject("ground Detector").transform;
            gd.parent = veicle;
            gd.localPosition = new Vector3(0, 0.1f, 0);


            Transform forward = new GameObject("forward").transform;
            forward.parent = veicle;
            forward.localPosition = new Vector3(0, 0.1f, 1.6f);


            veicle.gameObject.AddComponent<Rigidbody>();

            hoverCraft hoverCraftScript = veicle.gameObject.AddComponent<hoverCraft>();

            bool succesfullInput = false;
            if (tilting || input)
            {
                GameObject control = new GameObject("control");
                control.transform.parent = root;
                control.transform.position = model.position;

                if (tilting)
                {
                    hoverCraftTilt tilt = control.AddComponent<hoverCraftTilt>();
                    tilt.setup(hoverCraftScript, meshHold);
                }

                if (input)
                {
                    //InputWraper wraper = control.AddComponent<InputWraper>();
                    //wraper.setup(hoverCraftScript);
                    //succesfullInput = true;

                    Debug.Log("to auto setu new input system, import package in folder new input called: New Input System Example , and the remove comments above this message in code (3 lines).");
                }
            }

            if(cameraSetup)
            {
                Transform camPos = new GameObject("camPos").transform;
                camPos.parent = veicle;
                camPos.localPosition = new Vector3(0, 2f, -3);

                GameObject CHold = new GameObject("Cam Hold");
                CHold.transform.position = model.position;
                CHold.transform.parent = root;

                Transform camT = new GameObject("Camera").transform;
                camT.position = camPos.position;
                camT.LookAt(forward);
                camT.parent = CHold.transform;

                Camera cam = camT.gameObject.AddComponent<Camera>();

                camera camS = CHold.AddComponent<camera>();
                camS.setup(forward, cam.transform, camPos);
            }

            Transform p0 = null;
            if(hoverPoints)
            {
                Transform HHold = new GameObject("HoverPoint Hold").transform;
                HHold.parent = veicle;
                HHold.localPosition = Vector3.zero;

                p0 = new GameObject("p0").transform;
                Transform p1 = new GameObject("p1").transform;
                Transform p2 = new GameObject("p2").transform;
                Transform p3 = new GameObject("p3").transform;
                Transform p4 = new GameObject("p4").transform;
                Transform p5 = new GameObject("p5").transform;
                Transform p6 = new GameObject("p5").transform;

                p0.parent =
                p1.parent =
                p2.parent =
                p3.parent =
                p4.parent =
                p5.parent =
                p6.parent =
                    HHold;

                p0.localPosition = new Vector3(0.75f, 0.1f, 2.5f);
                p1.localPosition = new Vector3(-0.75f, 0.1f, 2.5f);
                p2.localPosition = new Vector3(0.75f, 0.1f, -2.5f);
                p3.localPosition = new Vector3(-0.75f, 0.1f, -2.5f);

                p4.localPosition = new Vector3(1.5f, 0.1f, 0);
                p5.localPosition = new Vector3(-1.5f, 0.1f, 0);

                p6.localPosition = new Vector3(0, 0.2f, 0);
            }

            hoverCraftScript.setup(center, forward, gd, p0, groundLayer, succesfullInput);
        }
    }
}
#endif