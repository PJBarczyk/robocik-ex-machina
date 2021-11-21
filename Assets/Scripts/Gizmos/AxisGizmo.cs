using System;
using UnityEngine;

namespace Gizmos
{
    public abstract class AxisGizmo : MonoBehaviour
    {

        /*  AxisGizmo is a gizmo that visually represents a floating point value in range of [-1, 1].
        By design, all given values are clamped to aforementioned range.
    */

        protected float _value;

        public void SetValue(float value)
        {
            _value = Mathf.Clamp(value, -1, 1);
            UpdateGizmo();
        }

        protected abstract void UpdateGizmo();
    }
}
