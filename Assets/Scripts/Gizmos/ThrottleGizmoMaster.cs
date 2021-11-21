using UnityEngine;

namespace Gizmos
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AUV))]
    public class ThrottleGizmoMaster : MonoBehaviour
    {
        [SerializeField] private AxisGizmo forwardThrottleGizmo;
        [SerializeField] private AxisGizmo sideThrottleGizmo;
        [SerializeField] private AxisGizmo verticalThrottleGizmo;
        [SerializeField] private AxisGizmo angularThrottleGizmo;

        private AUV _auv;

        void Start()
        {
            _auv = gameObject.GetComponent<AUV>();
        }

        private void Update() {
            forwardThrottleGizmo.    SetValue(_auv.forwardThrottle);
            sideThrottleGizmo.       SetValue(_auv.sideThrottle);
            verticalThrottleGizmo.   SetValue(_auv.verticalThrottle);
            angularThrottleGizmo.    SetValue(_auv.angularThrottle);
        }
    }
}
