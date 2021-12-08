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

        public int NumberOfSegments => points.Count / 3;

        public int NumberOfPoints => points.Count;

        public Vector2 this[int i] => points[i];

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
            points[pointIndex] = targetPos;

            if (pointIndex % 3 == 0)
            {
                if(pointIndex+1 < points.Count || _isClosed)
                    points[LoopIndex(pointIndex + 1)] += deltaMove;
                
                if(pointIndex-1 >= 0 || _isClosed)
                    points[LoopIndex(pointIndex - 1)] += deltaMove;
            }
            else
            {
                var nextPointIsAnchor = (pointIndex + 1) % 3 == 0;
                var correspondingControlIndex = (nextPointIsAnchor) ? pointIndex + 2 : pointIndex - 2;
                var anchorIndex = (nextPointIsAnchor) ? pointIndex + 1 : pointIndex - 1;

                if ((correspondingControlIndex < 0 || correspondingControlIndex >= points.Count) && !_isClosed) return;
                var distance = (points[LoopIndex(anchorIndex)] - points[LoopIndex(correspondingControlIndex)]).magnitude;
                var direction = (points[LoopIndex(anchorIndex)] - targetPos).normalized;

                points[LoopIndex(correspondingControlIndex)] = points[LoopIndex(anchorIndex)] + direction * distance;
            }
            
        }

        public void ToggleClosed()
        {
            _isClosed = !_isClosed;

            if (_isClosed)
            {
                var lastPointIndex = points.Count - 1;
                var secondFromLastIndex = points.Count - 2;
            
                points.Add(points[lastPointIndex]*2 - points[secondFromLastIndex]);
                points.Add(points[0]*2 - points[1]);
            }
            else
            {
                points.RemoveRange(points.Count-2,2);
            }
        }

        private int LoopIndex(int i)
        {
            return (i + points.Count) % points.Count;
        }

       
    }
}