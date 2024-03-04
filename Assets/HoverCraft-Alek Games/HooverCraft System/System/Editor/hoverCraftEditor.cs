using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlekGames.HoverCraftSystem.Systems.Main;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlekGames.Editor
{
    [CustomEditor(typeof(hoverCraft))]
    public class hoverCraftEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            hoverCraft hc = (hoverCraft)target;

            if(GUILayout.Button("if you like the asset, please leave a review on the asset store. it takes you 1 minute, but motivates me to make my assets better. click here to rate.", GUI.skin.textArea))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/physics/hovercraft-system-212594#reviews");
            }

            GUILayout.Space(25);



            DrawDefaultInspector();



            GUILayout.Space(10);

            if (GUILayout.Button("copy HoverPoint settings from hoverPoint 0"))
            {
                hc.setUpHoverPoint();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("find hover points from hoverPoint 0"))
            {
                hc.findHoverPointFromPoint0();
            }

            GUILayout.Space(30);

            GUILayout.Label("this sets up your rigidbody with values that should work. if they don't check your hoverPoints.", GUI.skin.box);
            if (GUILayout.Button("setup RigidBody"))
            {
                hc.rigidbodySetup();
            }
        }
    }
}
