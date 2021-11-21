using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Instantiate ScriptableObject/Physics model", fileName = "NewPhysicsModel")]
public class PhysicsModel : ScriptableObject
{
    [Min(0)] public float dragCoefficient;
    public bool linearApproximation = false;
    [Min(0)] public float angularDragCoefficient;
    
    [Space, Tooltip("Speed below the given threshold will be rounded down to zero.")]
    [Min(0)] public float minSpeed;
    [Min(0)] public float minAngularSpeed;

    public Vector3 GetDragForce(Vector3 velocity)
    {
        var drag = -velocity;
        drag.Scale(velocity);

        return drag * dragCoefficient;
    }

    public void ApplyDrag(ref Vector3 velocity)
    {
        var speed = velocity.magnitude;

        // Change in velocity due to drag, where drag = -v^2 * c
        var changeInSpeed =
            dragCoefficient
            * (linearApproximation ? speed : speed * speed)
            * -1
            * Time.deltaTime;

        if (speed + changeInSpeed <= minSpeed)
        {
            velocity = Vector3.zero;
        }
        else
        {
            velocity += velocity.normalized * changeInSpeed;
        }
    }

    public void ApplyAngularDrag(ref float angularVelocity)
    {
        angularVelocity -= angularVelocity * angularDragCoefficient * Time.deltaTime;
    }
}
