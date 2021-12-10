using UnityEngine;

namespace Curve.Scripts._2D
{
    public class PathCreator2D : MonoBehaviour
    {
        [HideInInspector]
        public Path2D path2D;

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
            path2D = new Path2D(transform.position);
        }

        private void Reset()
        {
            CreatePath();
        }
    }
}