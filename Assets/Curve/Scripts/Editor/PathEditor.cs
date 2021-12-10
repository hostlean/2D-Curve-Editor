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

            EditorGUI.BeginChangeCheck();
            
            if (GUILayout.Button("Create new"))
            {
                Undo.RecordObject(_creator,"Create new");
                _creator.CreatePath();
                _path = _creator.path;
            }

            var isClosed = GUILayout.Toggle(_path.IsClosed, "Closed");
            if (isClosed != _path.IsClosed)
            {
                Undo.RecordObject(_creator, "Toggle closed");
                _path.IsClosed = isClosed;
            }

            var autoSetControlPoints = GUILayout.Toggle(_path.AutoSetControlPoints, "Auto Set Control Points");

            if (autoSetControlPoints != _path.AutoSetControlPoints)
            {
                Undo.RecordObject(_creator, "Toggle auto set controls");
                _path.AutoSetControlPoints = autoSetControlPoints;
            }
            
            if(EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
        }

        private void OnEnable()
        {
            _creator = (PathCreator)target;
            if (_creator.path == null)
            {
                _creator.CreatePath();
            }

            _path = _creator.path;
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

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
            {
                var minDistanceToAnchor = .05f;
                var closestAnchorIndex = -1;

                for (var i = 0; i < _path.NumberOfPoints; i+=3)
                {
                    var distance = Vector2.Distance(mousePos, _path[i]);
                    if (distance < minDistanceToAnchor)
                    {
                        minDistanceToAnchor = distance;
                        closestAnchorIndex = i;
                    }
                }

                if (closestAnchorIndex != -1)
                {
                    Undo.RecordObject(_creator, "Delete segment");
                    _path.DeleteSegment(closestAnchorIndex);
                }
                
               
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