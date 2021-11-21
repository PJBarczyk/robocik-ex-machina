using UnityEngine;

namespace Gizmos
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class RadialSpriteAxisGizmo : AxisGizmo
    {
        [SerializeField] private Color positiveValueColor;
        [SerializeField] private Color negativeValueColor;

        [SerializeField, Range(0, 1)] private float maxProgress = 0.5f;

        private SpriteRenderer _renderer;
        private Material _material;
        
        private readonly string ShaderColorReference = "_Color";
        private readonly string ShaderProgressReference = "_Progress";
        private readonly string ShaderClockwiseReference = "_Clockwise";
    

        private void Start() {
            _renderer = gameObject.GetComponent<SpriteRenderer>();
            _material = _renderer.sharedMaterial;

            _material.SetFloat(ShaderProgressReference, 0);
        }

        protected override void UpdateGizmo()
        {
            if (_value < 0)
            {
                _renderer.flipX = true;
                _material.SetFloat(ShaderClockwiseReference, 0);
                _material.SetColor(ShaderColorReference, negativeValueColor);
            }
            else
            {
                _renderer.flipX = false;
                _material.SetFloat(ShaderClockwiseReference, 1);
                _material.SetColor(ShaderColorReference, positiveValueColor);
            }            

            _material.SetFloat(ShaderProgressReference, Mathf.Abs(_value) * maxProgress);
        }
    }
}
