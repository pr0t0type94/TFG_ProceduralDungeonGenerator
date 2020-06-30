using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGeneration))]
public class DungeonGenerator_CustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DungeonGeneration myTarget = (DungeonGeneration)target;

        if (GUILayout.Button("Build Dungeon"))
        {
            myTarget.GenerateDungeon(myTarget.m_graphFileToGenerate);
        }



    }
}


