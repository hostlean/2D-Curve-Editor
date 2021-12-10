using System;
using Curve.Scripts.Examples;
using UnityEngine;
using UnityEditor;

namespace Curve.Scripts.Editor
{
    [CustomEditor(typeof(RoadCreator))]
    public class RoadEditor : UnityEditor.Editor
    {
        private RoadCreator _creator;

        private void OnEnable()
        {
            _creator = (RoadCreator)target;
        }

        private void OnSceneGUI()
        {
            if (_creator.autoUpdate && Event.current.type == EventType.Repaint)
            {
                _creator.UpdateRoad();
            }
        }
    }
}