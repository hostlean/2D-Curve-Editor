using System;
using UnityEngine;

namespace Curve
{
    public class PathCreator : MonoBehaviour
    {
        public Path Path { get; private set; }

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        public void CreatePath()
        {
            Path = new Path(transform.position);
        }
    }
}