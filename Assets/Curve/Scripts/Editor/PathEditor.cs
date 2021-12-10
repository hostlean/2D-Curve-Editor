using System;
using UnityEditor;
using UnityEngine;

namespace Curve.Scripts.Editor
{
    [CustomEditor(typeof(PathCreator))]
    public class PathEditor : UnityEditor.Editor
    {
        private PathCreator _creator;
        private Path Path => _creator.path;

        private const float SegmentSelectDistanceThreshold = .1f;
        private int _selectedSegmentIndex = -1;
        
        private void OnEnable()
        {
            _creator = (PathCreator)target;
            if (_creator.path == null)
            {
                _creator.CreatePath();
            }
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
        }

        private void ClosedPathToggle()
        {
            var isClosed = GUILayout.Toggle(Path.IsClosed, "Closed");
            
            if (isClosed == Path.IsClosed) return;
            
            Undo.RecordObject(_creator, "Toggle closed");
            Path.IsClosed = isClosed;
        }

        private void AutoSetControlPointsToggle()
        {
            var autoSetControlPoints = GUILayout.Toggle(Path.AutoSetControlPoints, "Auto Set Control Points");

            if (autoSetControlPoints == Path.AutoSetControlPoints) return;
            
            Undo.RecordObject(_creator, "Toggle auto set controls");
            Path.AutoSetControlPoints = autoSetControlPoints;
        }

        #endregion

        #region Input

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
                    Path.SplitSegment(mousePos, _selectedSegmentIndex);
                }
                else if(!Path.IsClosed)
                {
                    Undo.RecordObject(_creator, "Add segment");
                    Path.AddSegment(mousePos);
                }
              
            }
        }

        private void DeleteSegmentInput(Event guiEvent, Vector2 mousePos)
        {
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
            {
                var minDistanceToAnchor = _creator.anchorDiameter * .5f;
                var closestAnchorIndex = -1;

                for (var i = 0; i < Path.NumberOfPoints; i += 3)
                {
                    var distance = Vector2.Distance(mousePos, Path[i]);
                    if (distance < minDistanceToAnchor)
                    {
                        minDistanceToAnchor = distance;
                        closestAnchorIndex = i;
                    }
                }

                if (closestAnchorIndex != -1)
                {
                    Undo.RecordObject(_creator, "Delete segment");
                    Path.DeleteSegment(closestAnchorIndex);
                }
            }
        }

        private void SplitSegmentCheck(Event guiEvent, Vector2 mousePos)
        {
            if (guiEvent.type != EventType.MouseMove) return;
            
            var minDistanceToSegment = SegmentSelectDistanceThreshold;
            var newSelectedSegmentIndex = -1;

            for (var i = 0; i < Path.NumberOfSegments; i++)
            {
                var points = Path.GetPointsInSegment(i);

                var distance =
                    HandleUtility.DistancePointBezier(
                        mousePos,
                        points[0],
                        points[3],
                        points[1],
                        points[2]);

                if (!(distance < minDistanceToSegment)) continue;
                
                minDistanceToSegment = distance;
                newSelectedSegmentIndex = i;
            }

            if (newSelectedSegmentIndex == _selectedSegmentIndex) return;
            
            _selectedSegmentIndex = newSelectedSegmentIndex;
            HandleUtility.Repaint();
        }

        #endregion

        #region Draw

         private void Draw()
        {
            for (var i = 0; i < Path.NumberOfSegments; i++)
            {
                var points = Path.GetPointsInSegment(i);

                DrawControlPointLines(points);
                
                DrawBezierLines(i, points);
            }

            DrawAnchorAndControlPoints();
        }

        private void DrawAnchorAndControlPoints()
        {
            for (var i = 0; i < Path.NumberOfPoints; i++)
            {
                if (i % 3 == 0 || _creator.displayControlPoints)
                {
                    Handles.color = (i % 3 == 0) ? _creator.anchorCol : _creator.cpCol;
                    var handleSize = (i % 3 == 0) ? _creator.anchorDiameter : _creator.controlPointDiameter;
                    Vector2 newPos = Handles.FreeMoveHandle(
                        Path[i],
                        Quaternion.identity,
                        handleSize,
                        Vector2.zero,
                        Handles.CylinderHandleCap);

                    if (Path[i] != newPos)
                    {
                        Undo.RecordObject(_creator, "Move Point");
                        Path.MovePoint(i, newPos);
                    }
                }
            }
        }

        private void DrawControlPointLines(Vector2[] points)
        {
            if (!_creator.displayControlPoints) return;
            
            Handles.color = Color.black;
            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[2], points[3]);
        }

        private void DrawBezierLines(int i, Vector2[] points)
        {
            var segmentColor = (i == _selectedSegmentIndex && Event.current.shift)
                ? _creator.selectedSegmentCol
                : _creator.segmentCol;

            Handles.DrawBezier(
                points[0],
                points[3],
                points[1],
                points[2],
                segmentColor,
                null,
                2);
        }

        #endregion

       
    }
}