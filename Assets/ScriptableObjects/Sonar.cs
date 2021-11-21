using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Instantiate ScriptableObject/Sonar")]
public class Sonar : ScriptableObject
{
    [Header("Transform")]
    public Vector3 localPosition;
    public Vector3 direction;

    [Space, Header("Line renderer")]
    public bool renderAsLine;
    public Material material;
    public Gradient gradient;
    public AnimationCurve widthCurve = AnimationCurve.Linear(1, 1, 1, 1);
    public float widthMult = .1f;
}
