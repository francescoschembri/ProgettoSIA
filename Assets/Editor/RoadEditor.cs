using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadCreator))]
public class RoadEditor : Editor
{
    RoadCreator creator;

    private bool update = false;

    

    private void OnSceneGUI()
    {
        if ((update || creator.autoUpdate) && Event.current.type == EventType.Repaint)
        {
            creator.SetupPath();
            creator.UpdateRoad();
            creator.UpdateRoad();
            update = false;
        }
    }

    public override void OnInspectorGUI() 
    {
        base.OnInspectorGUI();

        bool showWalls = GUILayout.Toggle(creator.showWalls, "Show walls");
        if (showWalls != creator.showWalls)
        {
            Undo.RecordObject(creator, "Show walls");
            creator.showWalls = showWalls;
            creator.ResetWalls(false);
        }

        bool genWalls = GUILayout.Toggle(creator.generateWalls, "Generate walls");
        if (genWalls != creator.generateWalls)
        {
            Undo.RecordObject(creator, "Generate walls");
            creator.generateWalls = genWalls;
            creator.ResetWalls(true);
        }

        bool autoUpdate = GUILayout.Toggle(creator.autoUpdate, "Auto update");
        if (autoUpdate != creator.autoUpdate)
        {
            creator.autoUpdate = autoUpdate;
        }

        if (GUILayout.Button("Update"))
        {
            update = true;
            SceneView.RepaintAll();
        }
    }

    private void OnEnable()
    {
        creator = (RoadCreator)target;
        creator.SetupPath();
    }
}
