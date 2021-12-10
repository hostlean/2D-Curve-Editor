using System;
using Curve.Scripts._2D;
using UnityEditor;
using UnityEngine;

namespace Curve.Scripts.Editor
{
    [CustomEditor(typeof(PathCreator2D))]
    public class PathEditor2D : UnityEditor.Editor
    {
        private PathCreator2D _creator2D;
        private Path2D Path2D => _creator2D.path2D;

        private const float SegmentSelectDistanceThreshold = .1f;
        private int _selectedSegmentIndex = -1;
        
        private void OnEnable()
        {
            _creator2D = (PathCreator2D)target;
            if (_creator2D.path2D == null)
            {
                _creator2D.CreatePath();
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
            
            Undo.RecordObject(_creator2D, "Create new");
            _creator2D.CreatePath();
        }

        private void ClosedPathToggle()
        {
            var isClosed = GUILayout.Toggle(Path2D.IsClosed, "Closed");
            
            if (isClosed == Path2D.IsClosed) return;
            
            Undo.RecordObject(_creator2D, "Toggle closed");
            Path2D.IsClosed = isClosed;
        }

        private void AutoSetControlPointsToggle()
        {
            var autoSetControlPoints = GUILayout.Toggle(Path2D.AutoSetControlPoints, "Auto Set Control Points");

            if (autoSetControlPoints == Path2D.AutoSetControlPoints) return;
            
            Undo.RecordObject(_creator2D, "Toggle auto set controls");
            Path2D.AutoSetControlPoints = autoSetControlPoints;
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
            
            HandleUtility.AddDefaultControl(0);
           
        }

        private void AddSegmentInput(Event guiEvent, Vector2 mousePos)
        {
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                if (_selectedSegmentIndex != -1)
                {
                    Undo.RecordObject(_creator2D, "Split segment");
                    Path2D.SplitSegment(mousePos, _selectedSegmentIndex);
                }
                else if(!Path2D.IsClosed)
                {
                    Undo.RecordObject(_creator2D, "Add segment");
                    Path2D.AddSegment(mousePos);
                }
              
            }
        }

        private void DeleteSegmentInput(Event guiEvent, Vector2 mousePos)
        {
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
            {
                var minDistanceToAnchor = _creator2D.anchorDiameter * .5f;
                var closestAnchorIndex = -1;

                for (var i = 0; i < Path2D.NumberOfPoints; i += 3)
                {
                    var distance = Vector2.Distance(mousePos, Path2D[i]);
                    if (distance < minDistanceToAnchor)
                    {
                        minDistanceToAnchor = distance;
                        closestAnchorIndex = i;
                    }
                }

                if (closestAnchorIndex != -1)
                {
                    Undo.RecordObject(_creator2D, "Delete segment");
                    Path2D.DeleteSegment(closestAnchorIndex);
                }
            }
        }

        private void SplitSegmentCheck(Event guiEvent, Vector2 mousePos)
        {
            if (guiEvent.type != EventType.MouseMove) return;
            
            var minDistanceToSegment = SegmentSelectDistanceThreshold;
            var newSelectedSegmentIndex = -1;

            for (var i = 0; i < Path2D.NumberOfSegments; i++)
            {
                var points = Path2D.GetPointsInSegment(i);

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
            for (var i = 0; i < Path2D.NumberOfSegments; i++)
            {
                var points = Path2D.GetPointsInSegment(i);

                DrawControlPointLines(points);
                
                DrawBezierLines(i, points);
            }

            DrawAnchorAndControlPoints();
        }

        private void DrawAnchorAndControlPoints()
        {
            for (var i = 0; i < Path2D.NumberOfPoints; i++)
            {
                if (i % 3 == 0 || _creator2D.displayControlPoints)
                {
                    Handles.color = (i % 3 == 0) ? _creator2D.anchorCol : _creator2D.cpCol;
                    var handleSize = (i % 3 == 0) ? _creator2D.anchorDiameter : _creator2D.controlPointDiameter;
                    Vector2 newPos = Handles.FreeMoveHandle(
                        Path2D[i],
                        Quaternion.identity,
                        handleSize,
                        Vector2.zero,
                        Handles.CylinderHandleCap);

                    if (Path2D[i] != newPos)
                    {
                        Undo.RecordObject(_creator2D, "Move Point");
                        Path2D.MovePoint(i, newPos);
                    }
                }
            }
        }

        private void DrawControlPointLines(Vector2[] points)
        {
            if (!_creator2D.displayControlPoints) return;
            
            Handles.color = Color.black;
            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[2], points[3]);
        }

        private void DrawBezierLines(int i, Vector2[] points)
        {
            var segmentColor = (i == _selectedSegmentIndex && Event.current.shift)
                ? _creator2D.selectedSegmentCol
                : _creator2D.segmentCol;

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