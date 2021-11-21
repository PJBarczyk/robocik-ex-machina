using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraMaster : MonoBehaviour
{
    public Transform target;

    public AnimationCurve zoomCurve;
    [Min (.001f)] public float zoomMaxDistance = 20;
    [Min (.001f)] public float zoomMinDistance = 1;
    
    private bool _rotationModeEnabled = false;
    public bool rotationLock = false;
    
    public float zoom;
    private Vector3 _basePosition;
    private Vector3 _baseRotation;

    private void LateUpdate()
    {
        var zoomDistance = Mathf.Lerp(zoomMaxDistance, zoomMinDistance, zoomCurve.Evaluate(zoom));
        
        var rotation = rotationLock ? target.eulerAngles + _baseRotation  : _baseRotation;

        var zoomTransform = Quaternion.Euler(rotation) * Vector3.forward * zoomDistance * -1;

        var trans = transform;
        trans.position = target.position + zoomTransform;;
        trans.eulerAngles = _baseRotation;
        
    }

    #region InputHandling

    public void OnCameraZoom(InputAction.CallbackContext value)
    {
        float input = value.ReadValue<float>();
        
        zoom = Mathf.Clamp(zoom + input, 0, 1);
    }
    
    public void OnCameraRotation(InputAction.CallbackContext value)
    {
        if (_rotationModeEnabled)
        {
            var input = value.ReadValue<Vector2>();

            _baseRotation = new Vector3(
                Mathf.Clamp(_baseRotation.x + input.y, -90, 90), 
                _baseRotation.y + input.x, 
                0);
        }
    }
    
    public void OnCameraRotationMode(InputAction.CallbackContext value)
    {
        switch (value.phase)
        {
            case InputActionPhase.Started:
                _rotationModeEnabled = true;
                break;
            
            case InputActionPhase.Canceled:
                _rotationModeEnabled = false;
                break;
        }
    }

    public void OnRotationLock(ChangeEvent<bool> evt)
    {
        if (evt.newValue == true)
        {
            rotationLock = true;
            _baseRotation -= target.eulerAngles;
        }
        else
        {
            rotationLock = false;
            _baseRotation = transform.rotation.eulerAngles;
        }
    }

    public void SetCameraZoom(ChangeEvent<float> evt)
    {
        zoom = evt.newValue;
    }

    #endregion


}
