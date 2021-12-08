using System;
using UnityEditor;
using UnityEngine;

namespace Curve.Scripts.Editor
{
    [CustomEditor(typeof(PathCreator))]
    public class PathEditor : UnityEditor.Editor
    {
        private PathCreator _creator;
        private Path _path;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Create New"))
            {
                _creator.CreatePath();
                _path = _creator.Path;
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Toggle Closed"))
            {
                _path.ToggleClosed();
                SceneView.RepaintAll();
            }
        }

        private void OnEnable()
        {
            _creator = (PathCreator)target;
            if (_creator.Path == null)
            {
                _creator.CreatePath();
            }

            _path = _creator.Path;
        }

        private void Input()
        {
            Event guiEvent = Event.current;

            Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                Undo.RecordObject(_creator, "Add segment");
                _path.AddSegment(mousePos);
            }
        }

        private void Draw()
        {
            for (var i = 0; i < _path.NumberOfSegments; i++)
            {
                Vector2[] points = _path.GetPointsInSegment(i);
                
                Handles.color = Color.black;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
                
                Handles.DrawBezier(
                    points[0], 
                    points[3], 
                    points[1], 
                    points[2], 
                    Color.green, 
                    null, 
                    2);
                
            }
            
            
            Handles.color = Color.red;
            for (var i = 0; i < _path.NumberOfPoints; i++)
            {
                Vector2 newPos = Handles.FreeMoveHandle(
                    _path[i], 
                    Quaternion.identity, 
                    .1f, 
                    Vector2.zero,
                    Handles.CylinderHandleCap);

                if (_path[i] != newPos)
                {
                    Undo.RecordObject(_creator, "Move Point");
                    _path.MovePoint(i, newPos);
                }
            }
        }

        private void OnSceneGUI()
        {
            Input();
            Draw();
        }
    }
}