using System;
using System.Collections.Generic;
using UnityEngine;

namespace Curve
{
    [Serializable]
    public class Path
    {
        [SerializeField] [HideInInspector] private List<Vector2> points;
        
        public int NumberOfSegments => (points.Count - 4) / 3 + 1;

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
                points[segmentIndex * 3 + 3]
            };
        }

        public void MovePoint(int pointIndex, Vector2 targetPos)
        {
            Vector2 deltaMove = targetPos - points[pointIndex];
            points[pointIndex] = targetPos;

            if (pointIndex % 3 == 0)
            {
                if(pointIndex+1 < points.Count)
                    points[pointIndex + 1] += deltaMove;
                
                if(pointIndex-1 >= 0)
                    points[pointIndex - 1] += deltaMove;
            }
            else
            {
                bool nextPointIsAnchor = (pointIndex + 1) % 3 == 0;
                int correspondingControlIndex = (nextPointIsAnchor) ? pointIndex + 2 : pointIndex - 2;
                int anchorIndex = (nextPointIsAnchor) ? pointIndex + 1 : pointIndex - 1;

                if (correspondingControlIndex < 0 || correspondingControlIndex >= points.Count) return;
                
                float distance = (points[anchorIndex] - points[correspondingControlIndex]).magnitude;
                Vector2 direction = (points[anchorIndex] - targetPos).normalized;

                points[correspondingControlIndex] = points[anchorIndex] + direction * distance;
            }
            
        }

       
    }
}