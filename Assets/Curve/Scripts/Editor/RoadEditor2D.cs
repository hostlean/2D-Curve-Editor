using System;
using Curve.Scripts.Examples;
using UnityEngine;
using UnityEditor;

namespace Curve.Scripts.Editor
{
    [CustomEditor(typeof(RoadCreator2D))]
    public class RoadEditor2D : UnityEditor.Editor
    {
        private RoadCreator2D _creator2D;

        private void OnEnable()
        {
            _creator2D = (RoadCreator2D)target;
        }

        private void OnSceneGUI()
        {
            if (_creator2D.autoUpdate && Event.current.type == EventType.Repaint)
            {
                _creator2D.UpdateRoad();
            }
        }
    }
}