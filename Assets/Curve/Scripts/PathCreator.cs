using System;
using UnityEngine;

namespace Curve
{
    public class PathCreator : MonoBehaviour
    {
        [HideInInspector]
        public Path path;

        public Color anchorCol = Color.red;
        public Color cpCol = Color.white;
        public Color segmentCol = Color.green;
        public Color selectedSegmentCol = Color.yellow;

        public float anchorDiameter = .1f;
        public float controlPointDiameter = .075f;

        public bool displayControlPoints = true;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        public void CreatePath()
        {
            path = new Path(transform.position);
        }

        private void Reset()
        {
            CreatePath();
        }
    }
}