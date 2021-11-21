using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Instantiate ScriptableObject/Thrust model", fileName = "NewThrustModel")]
public class ThrottleToThrustModel : ScriptableObject
{
    [Header("Linear thrust")]
    [Min(0)] public float baseLinearThrust;
    
    [Space]
    
    [Min(0)] public float frontThrustMult = 1;
    [Min(0)] public float backThrustMult = 1;

    [Space]
    
    [Min(0)] public float leftThrustMult = 1;
    [Min(0)] public float rightThrustMult = 1;

    [Space]
    
    [Min(0)] public float upwardThrustMult = 1;
    [Min(0)] public float downwardThrustMult = 1;

    [Space]
    [Header("Angular thrust")]
    [Min(0)] public float baseAngularThrust;
    
    [Space]
    
    [Min(0)] public float counterclockwiseThrustMult = 1;
    [Min(0)] public float clockwiseThrustMult = 1;
    
    public Vector3 GetLocalThrust(Vector3 throttle)
    {
        var thrust = new Vector3(
            throttle.x < 0 ? leftThrustMult : rightThrustMult,
            throttle.y < 0 ? downwardThrustMult : upwardThrustMult,
            throttle.z < 0 ? backThrustMult : frontThrustMult);

        thrust *= baseLinearThrust;
        thrust.Scale(throttle);

        return thrust;
    }

    public float GetAngularThrust(float angularThrottle)
    {
        return angularThrottle
               * baseAngularThrust
               * (angularThrottle < 0 ? counterclockwiseThrustMult : clockwiseThrustMult);
    }
}
