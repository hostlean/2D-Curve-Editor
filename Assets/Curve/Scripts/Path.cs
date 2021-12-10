using System;
using System.Collections.Generic;
using UnityEngine;

namespace Curve
{
    [Serializable]
    public class Path
    {
        [SerializeField] [HideInInspector] private List<Vector2> points;
        [SerializeField] [HideInInspector] private bool _isClosed;
        [SerializeField] [HideInInspector] private bool _autoSetControlPoints;

        public int NumberOfSegments => points.Count / 3;

        public int NumberOfPoints => points.Count;

        public Vector2 this[int i] => points[i];

        public bool AutoSetControlPoints
        {
            get => _autoSetControlPoints;
            set
            {
                if (_autoSetControlPoints != value)
                {
                    _autoSetControlPoints = value;
                    
                    if(_autoSetControlPoints)
                        AutoSetAllControlPoints();
                }
            }
        }

        public bool IsClosed
        {
            get => _isClosed;

            set
            {
                if (_isClosed == value) return;
                
                _isClosed = value;

                if (_isClosed)
                {
                    var lastPointIndex = points.Count - 1;
                    var secondFromLastIndex = points.Count - 2;
            
                    points.Add(points[lastPointIndex]*2 - points[secondFromLastIndex]);
                    points.Add(points[0]*2 - points[1]);

                    if (_autoSetControlPoints)
                    {
                        AutoSetAnchorControlPoints(0);
                        AutoSetAnchorControlPoints(points.Count - 3);
                    }
                }
                else
                {
                    points.RemoveRange(points.Count-2,2);

                    if (_autoSetControlPoints)
                    {
                        AutoSetStartAndEndControls();
                    }
                }
            }
        }

        public Path(Vector2 center)
        {
            points = new List<Vector2>
            {
                center + Vector2.left,
                center + (Vector2.left + Vector2.up) * .5f,
                center + (Vector2.right + Vector2.down) * .5f,
                center + Vector2.right
            };
        }

        public void AddSegment(Vector2 anchorPos)
        {
            var lastPointIndex = points.Count - 1;
            var secondFromLastIndex = points.Count - 2;
            
            points.Add(points[lastPointIndex]*2 - points[secondFromLastIndex]);
            
            points.Add((points[lastPointIndex] + anchorPos) * .5f);
            
            points.Add(anchorPos);

            if (_autoSetControlPoints)
            {
                AutoSetAllEffectedControlPoints(points.Count - 1);
            }
        }

        public void SplitSegment(Vector2 anchorPos, int segmentIndex)
        {
            
        }
        

        public void DeleteSegment(int anchorIndex)
        {
            if (NumberOfSegments <= 2 && (_isClosed || NumberOfSegments <= 1)) return;
            if (anchorIndex == 0)
            {
                if (_isClosed)
                {
                    points[points.Count - 1] = points[2];
                }

                points.RemoveRange(0, 3);
            }
            else if (anchorIndex == points.Count - 1 && !_isClosed)
            {
                points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3);
            }
        }

        public Vector2[] GetPointsInSegment(int segmentIndex)
        {
            return new Vector2[]
            {
                points[segmentIndex * 3], 
                points[segmentIndex * 3 + 1], 
                points[segmentIndex * 3 + 2],
                points[LoopIndex(segmentIndex * 3 + 3)]
            };
        }

        public void MovePoint(int pointIndex, Vector2 targetPos)
        {
            var deltaMove = targetPos - points[pointIndex];

            if (pointIndex % 3 != 0 && _autoSetControlPoints) return;
            points[pointIndex] = targetPos;

            if (_autoSetControlPoints)
            {
                AutoSetAllEffectedControlPoints(pointIndex);
            }

            if (pointIndex % 3 == 0)
            {
                if (pointIndex + 1 < points.Count || _isClosed)
                    points[LoopIndex(pointIndex + 1)] += deltaMove;

                if (pointIndex - 1 >= 0 || _isClosed)
                    points[LoopIndex(pointIndex - 1)] += deltaMove;
            }
            else
            {
                var nextPointIsAnchor = (pointIndex + 1) % 3 == 0;
                var correspondingControlIndex = (nextPointIsAnchor) ? pointIndex + 2 : pointIndex - 2;
                var anchorIndex = (nextPointIsAnchor) ? pointIndex + 1 : pointIndex - 1;

                if ((correspondingControlIndex < 0 || correspondingControlIndex >= points.Count) &&
                    !_isClosed) return;
                var distance = (points[LoopIndex(anchorIndex)] - points[LoopIndex(correspondingControlIndex)])
                    .magnitude;
                var direction = (points[LoopIndex(anchorIndex)] - targetPos).normalized;

                points[LoopIndex(correspondingControlIndex)] =
                    points[LoopIndex(anchorIndex)] + direction * distance;
            }

        }

        private void AutoSetAllEffectedControlPoints(int updatedAnchorIndex)
        {
            for (var i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i+=3)
            {
                if (i >= 0 && i < points.Count || _isClosed)
                {
                    AutoSetAnchorControlPoints(LoopIndex(i));
                }
            }
            AutoSetStartAndEndControls();
        }

        private void AutoSetAllControlPoints()
        {
            for (var i = 0; i < points.Count; i+=3)
            {
                AutoSetAnchorControlPoints(i);
            }
            AutoSetStartAndEndControls();
        }

        private void AutoSetAnchorControlPoints(int anchorIndex)
        {
            var anchorPos = points[anchorIndex];
            var direction = Vector2.zero;

            var neighbourDistances = new float[2];
            
            if (anchorIndex - 3 >= 0 || _isClosed)
            {
                var offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
                direction += offset.normalized;
                neighbourDistances[0] = offset.magnitude;
            }
            
            if (anchorIndex - 3 >= 0 || _isClosed)
            {
                var offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
                direction -= offset.normalized;
                neighbourDistances[1] = -offset.magnitude;
            }

            direction.Normalize();
            for (var i = 0; i < 2; i++)
            {
                var controlIndex = anchorIndex + i * 2 - 1;
                if (controlIndex >= 0 && controlIndex < points.Count || _isClosed)
                {
                    points[LoopIndex(controlIndex)] = anchorPos + direction * neighbourDistances[i] * .5f;
                }
            }
        }

        private void AutoSetStartAndEndControls()
        {
            if (!_isClosed)
            {
                points[1] = (points[0] + points[2]) * .5f;
                points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * .5f;
            }
        }

        private int LoopIndex(int i)
        {
            return (i + points.Count) % points.Count;
        }

       
    }
}