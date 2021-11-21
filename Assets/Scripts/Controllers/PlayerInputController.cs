using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers
{
    public class PlayerInputController : AUVController
    {
        [HideInInspector] public AUV auv;

        private void Start() {
            auv = gameObject.GetComponent<AUV>();
        }

        public void OnForwardThrottle(InputAction.CallbackContext context)
        {
            auv.forwardThrottle = context.ReadValue<float>();
        }
        public void OnSideThrottle(InputAction.CallbackContext context)
        {
            auv.sideThrottle = context.ReadValue<float>();
        }
        public void OnVerticalThrottle(InputAction.CallbackContext context)
        {
            auv.verticalThrottle = context.ReadValue<float>();
        }
        public void OnAngularThrottle(InputAction.CallbackContext context)
        {
            auv.angularThrottle = context.ReadValue<float>();
        }
    }
}
