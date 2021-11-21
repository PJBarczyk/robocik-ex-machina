using System;
using UnityEngine;

namespace Gizmos
{
    public class ScalingAxisGizmo : AxisGizmo
    {
        [SerializeField] private Transform positive;
        [SerializeField] private Transform negative;

        private Vector3 _originalSizePositive;
        private Vector3 _originalSizeNegative;

        private void Start()
        {
            _originalSizePositive = positive.localScale;
            _originalSizeNegative = negative.localScale;
        }
        protected override void UpdateGizmo()
        {
            if (_value < 0)
            {
                positive.localScale = Vector3.zero;
                negative.localScale = _originalSizeNegative * -_value;
            }
            else
            {
                positive.localScale = _originalSizePositive * _value;
                negative.localScale = Vector3.zero;
            }
        }
    }
}
