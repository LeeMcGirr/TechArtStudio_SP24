using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AlekGames.Editor
{
    
    public class OverviewEditorWindow : EditorWindow
    {
       [MenuItem("Window/Alek Games/HoverCraft System/Overview + Help")]
        public static void showWindow()
        {
            GetWindow<OverviewEditorWindow>("Overview + Help");

        }

        string[] tabs = { "Setup", "Documentation", "Help + Support" , "Update Log"};
        static int tab;

        private void OnGUI()
        {
            tab = GUILayout.SelectionGrid(tab, tabs, 2);

            GUILayout.Space(20);

            switch(tab)
            {
                case 0:
                    drawSetup();
                    break;
                case 1:
                    drawDocumentation();
                    break;
                case 2:
                    drawHelpSupport();
                    break;
                case 3:
                    drawUpdateLog();
                    break;
            }
        }

        private void drawSetup()
        {
            EditorGUILayout.HelpBox("video links are in the Help + Support tab", MessageType.Info);

            GUILayout.Space(10);

            EditorGUILayout.HelpBox("setup guide is in: Alek Games/HoverCraft System  in file HowTo. read it to get better understanding of the system", MessageType.Info);

            GUILayout.Space(15);

            if(GUILayout.Button("Open HoverCraft Maker")) GetWindow<hoverCraftEditorWindow>("hoverCraft Maker");
        }

        private void drawDocumentation()
        {
            EditorGUILayout.HelpBox("documentation is in: Alek Games/HoverCraft System/", MessageType.Info);
            GUILayout.Space(20);
            EditorGUILayout.HelpBox("detailed documentation in file Documentation. you can read it if you are looking for more info on what a specific value does. it is all there", MessageType.Info);
            GUILayout.Space(20);
            EditorGUILayout.HelpBox("more on how to learn how to setUp and use the system is in in file ReadMe.", MessageType.Info);
        }

        private void drawHelpSupport()
        {
            EditorGUILayout.HelpBox("there are tips for the system in the documentation of the system. specificly the HowTo file", MessageType.Info);
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("if you need help do not hesitate to email me at: alekgamesunity@gmail.com .", MessageType.Info);
            GUILayout.Space(10);

            if (GUILayout.Button("HoverCraft System basics video"))
            {
                //Debug.Log("I have not linked this button to a tutorial video. please email me about this");
                Application.OpenURL("https://youtu.be/v2fXYZMiY0M");
            }

            if (GUILayout.Button("HoverCraft System grid and counter movement video"))
            {
                //Debug.Log("I have not linked this button to a tutorial video. please email me about this");
                Application.OpenURL("https://youtu.be/-j3B6jlNqfA");
            }

            if (GUILayout.Button("tire symulation video"))
            {
                //Debug.Log("I have not linked this button to a tutorial video. please email me about this");
                Application.OpenURL("https://youtu.be/3mQyCH642UE");
            }
        }

        Vector2 scrollAll;
        int updatesAmmount = 5;

        private void drawUpdateLog()
        {
            Rect rectPos = EditorGUILayout.GetControlRect();
            Rect rectBox = new Rect(rectPos.x, rectPos.y, rectPos.width, 1000);
            Rect rectViev = new Rect(rectPos.x, rectPos.y, rectPos.width, updatesAmmount * 300);
            scrollAll = GUI.BeginScrollView(rectBox, scrollAll, rectViev, false, false);
            ///////////////////
            ///

            string[] v2 = { "tire simulation", "turn types" };
            drawUpdate("v2", "tires", v2);

            string[] v14 = { "grid spawner", "fixes to tilting and hoverCraft itself", "custom counter forces", "bug fixes" };
            drawUpdate("v1.4", "Grid + counter forces", v14);

            string[] v13 = { "zone Input", "moved hoverCraft Maker to Tools"};
            drawUpdate("v1.3", "VR controlls support", v13);

            string[] v12 = { "HoverCraft Maker", "thrust change controll", "Overview + Help window" };
            drawUpdate("v1.2", "mostly based on adding help for user", v12);

            string[] v11 = { "tilting", "new input system examples" };
            drawUpdate("v1.1", "finally added tilting", v11);

            string[] v101 = { "max noraml float angles"};
            drawUpdate("v1.01", "update mainly focused on adding a few nw values", v101);

            string[] v1 = { "floating", "turning", "jumping" };
            drawUpdate("v1", "the first version of the system. mainly focused on just making the hoverCraft float", v1);

            ///
            ///////////////////
            GUI.EndScrollView();
        }

        private void drawUpdate(string version, string basicText, string[] updatededThings)
        {
            
            GUILayout.Space(10);
            GUILayout.BeginVertical("window");
            GUILayout.Label(version, GUI.skin.box);


            GUILayout.Label(basicText, EditorStyles.boldLabel);

            for (int i = 0; i < updatededThings.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);
                GUILayout.Label(" - " + updatededThings[i]);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
    }
}
