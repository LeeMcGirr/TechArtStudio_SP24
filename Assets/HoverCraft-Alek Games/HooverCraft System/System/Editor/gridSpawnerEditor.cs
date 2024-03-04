using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AlekGames.HoverCraftSystem.Systems.Addons;

namespace AlekGames.Editor
{
    [CustomEditor(typeof(gridSpawner))]
    public class gridSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            gridSpawner g = (gridSpawner)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(g.hoverCraftScript)));
            if(g.hoverCraftScript == null) EditorGUILayout.HelpBox("grid is not going to be added to hoverCraft as hoverPoints", MessageType.Info);       
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(g.pointObj)));
            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(g.shape)));

            if(g.shape == gridSpawner.shapeT.square)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(g.xSpawn)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(g.zSpawn)));
                GUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(g.xLen)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(g.zLen)));
                GUILayout.Label("full points count: " + g.xSpawn * g.zSpawn, EditorStyles.boldLabel);
            }
            else //if(g.shape == gridSpawner.shapeT.circle)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(g.borderSpawn)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(g.layersCount)));
                GUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(g.radius)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(g.up)));
                GUILayout.Space(10);
                GUILayout.Label("full points count: " + g.borderSpawn * g.layersCount, EditorStyles.boldLabel);
            }

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(20);

            if (GUILayout.Button("spawn")) g.spawnGrid();
        }
    }
}
