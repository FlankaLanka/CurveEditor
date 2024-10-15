using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    private PathCreator creator;
    private Path path;

    private float segmentSelectThreshold = 0.1f;
    private int selectedSegmentIndex = -1;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Create New"))
        {
            Undo.RecordObject(creator, "Toggle Auto Set Controls");
            creator.CreatePath();
            path = creator.path;
        }

        if(GUILayout.Button("Toggle Closed Path"))
        {
            Undo.RecordObject(creator, "Toggle Auto Set Controls");
            path.ToggleClosed();
        }

        bool autoSetControlPoints = GUILayout.Toggle(path.AutoSetControlPoints, "Auto Set Control Points");
        if(autoSetControlPoints != path.AutoSetControlPoints)
        {
            Undo.RecordObject(creator, "Toggle Auto Set Controls");
            path.AutoSetControlPoints = autoSetControlPoints;
        }

        if(EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        Input();
        Draw();
    }

    void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Undo.RecordObject(creator, "Add Segment");
            path.AddSegment(mousePos);
        }

        //find the closest point to delete
        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDistToAnchor = 0.05f;
            int closestAnchorIndex = -1;

            for(int i = 0; i < path.NumPoints; i+=3)
            {
                float curDist = Vector2.Distance(mousePos, path[i]);
                if(curDist < minDistToAnchor)
                {
                    minDistToAnchor = curDist;
                    closestAnchorIndex = i;
                }
            }

            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(creator, "Delete Segment");
                path.DeleteSegment(closestAnchorIndex);
            }
        }

        
    }

    private void Draw()
    {
        Handles.color = Color.white;
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector2[] points = path.GetPointsInSegment(i);
            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.magenta, null, 1.0f);
        }

        for(int i = 0; i < path.NumPoints; i++)
        {
            if(i % 3 == 0)
                Handles.color = Color.cyan;
            else
                Handles.color = Color.yellow;

            Vector2 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, 0.1f, Vector2.zero, Handles.CylinderHandleCap);
            if (path[i] != newPos)
            {
                Undo.RecordObject(creator, "Move Point");
                path.MovePoint(i, newPos);
            }
        }
    }

    private void OnEnable()
    {
        creator = (PathCreator)target;
        if(creator.path == null)
        {
            creator.CreatePath();
        }
        path = creator.path;
    }

}
