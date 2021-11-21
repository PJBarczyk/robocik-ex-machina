using System;
using UnityEngine;

namespace Gizmos
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class LinearMeshAxisGizmo : AxisGizmo
    {
        private MeshRenderer _renderer;
        private Material _material;
        
        [SerializeField] private Vector3 axisDirection = Vector3.forward;
        [SerializeField] private Vector3 meshScale = Vector3.one;
        [SerializeField] private Vector3 positionOffset = Vector3.zero;
        


        [Header("Visuals")]
        [SerializeField] private Color positiveValueColor;
        [SerializeField] private Color negativeValueColor;
        
        private void Start()
        {
            _renderer = gameObject.GetComponent<MeshRenderer>();
            _material = _renderer.material; // cannot use sharedMaterial, as other linear mesh gizmos could use the same material
                
            transform.LookAt(axisDirection, Vector3.up);
        }

        private void OnValidate()
        {
            UpdateGizmo();
        }

        protected override void UpdateGizmo()
        {
            _material.color = _value < 0 ? negativeValueColor : positiveValueColor;

            var scale = Vector3.one * _value;
            scale.Scale(meshScale);

            transform.localScale = scale;
            transform.localPosition = _value < 0 ? Quaternion.Euler(0, 180, 0) * positionOffset : positionOffset;
        }
    }
}
