using UnityEngine;

namespace Curve.Scripts.Examples
{
    public class PathPlacer : MonoBehaviour
    {
        public float spacing = .1f;
        public float resolution = 1f;

        private void Start()
        {
            var pathCreator = FindObjectOfType<PathCreator>();
            var path = pathCreator.path;
            var points = path.CalculateEvenlySpacedPoints(spacing, resolution);

            foreach (var point in points)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = point;
                go.transform.localScale = Vector3.one * spacing * .5f;
            }

        }
    }
}