using System;
using UnityEngine;

namespace Curve
{
    public class PathCreator : MonoBehaviour
    {
        [HideInInspector]
        public Path path;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        public void CreatePath()
        {
            path = new Path(transform.position);
        }
    }
}