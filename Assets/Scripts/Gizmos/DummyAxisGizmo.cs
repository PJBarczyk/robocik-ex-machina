using System;
using UnityEngine;

namespace Gizmos
{
    public class DummyAxisGizmo : AxisGizmo
    {
        private void Start()
        {
            Debug.LogWarning("A DummyAxisGizmo is in use.");
        }

        protected override void UpdateGizmo()
        {
            // Dummy does nothing, duh
        }
    }
}
