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

        private const float SegmentSelectDistanceThreshold = .1f;
        private int _selectedSegmentIndex = -1;
        
        private void OnEnable()
        {
            _creator = (PathCreator)target;
            if (_creator.path == null)
            {
                _creator.CreatePath();
            }

            _path = _creator.path;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            
            CreateNewPathButton();

            ClosedPathToggle();

            AutoSetControlPointsToggle();

            if(EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
        }

        private void OnSceneGUI()
        {
            Input();
            Draw();
        }

        #region Inspector

        private void CreateNewPathButton()
        {
            if (!GUILayout.Button("Create new")) return;
            
            Undo.RecordObject(_creator, "Create new");
            _creator.CreatePath();
            _path = _creator.path;
        }

        private void ClosedPathToggle()
        {
            var isClosed = GUILayout.Toggle(_path.IsClosed, "Closed");
            
            if (isClosed == _path.IsClosed) return;
            
            Undo.RecordObject(_creator, "Toggle closed");
            _path.IsClosed = isClosed;
        }

        private void AutoSetControlPointsToggle()
        {
            var autoSetControlPoints = GUILayout.Toggle(_path.AutoSetControlPoints, "Auto Set Control Points");

            if (autoSetControlPoints == _path.AutoSetControlPoints) return;
            
            Undo.RecordObject(_creator, "Toggle auto set controls");
            _path.AutoSetControlPoints = autoSetControlPoints;
        }

        #endregion
        
        

        private void Input()
        {
            var guiEvent = Event.current;

            Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

            AddSegmentInput(guiEvent, mousePos);

            DeleteSegmentInput(guiEvent, mousePos);

            SplitSegmentCheck(guiEvent, mousePos);
           
        }

        private void AddSegmentInput(Event guiEvent, Vector2 mousePos)
        {
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                if (_selectedSegmentIndex != -1)
                {
                    Undo.RecordObject(_creator, "Split segment");
                    _path.SplitSegment(mousePos, _selectedSegmentIndex);
                }
                else if(!_path.IsClosed)
                {
                    Undo.RecordObject(_creator, "Add segment");
                    _path.AddSegment(mousePos);
                }
              
            }
        }

        private void DeleteSegmentInput(Event guiEvent, Vector2 mousePos)
        {
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
            {
                var minDistanceToAnchor = .05f;
                var closestAnchorIndex = -1;

                for (var i = 0; i < _path.NumberOfPoints; i += 3)
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

        private void SplitSegmentCheck(Event guiEvent, Vector2 mousePos)
        {
            if (guiEvent.type == EventType.MouseMove)
            {
                var minDistanceToSegment = SegmentSelectDistanceThreshold;
                var newSelectedSegmentIndex = -1;

                for (var i = 0; i < _path.NumberOfSegments; i++)
                {
                    var points = _path.GetPointsInSegment(i);

                    var distance =
                        HandleUtility.DistancePointBezier(
                            mousePos,
                            points[0],
                            points[3],
                            points[1],
                            points[2]);

                    if (distance < minDistanceToSegment)
                    {
                        minDistanceToSegment = distance;
                        newSelectedSegmentIndex = i;
                    }
                }

                if (newSelectedSegmentIndex != _selectedSegmentIndex)
                {
                    _selectedSegmentIndex = newSelectedSegmentIndex;
                    HandleUtility.Repaint();
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

                Color segmentColor = (i == _selectedSegmentIndex && Event.current.shift) ? Color.red : Color.green;
                
                Handles.DrawBezier(
                    points[0], 
                    points[3], 
                    points[1], 
                    points[2], 
                    segmentColor, 
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
    }
}