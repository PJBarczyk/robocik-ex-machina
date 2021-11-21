using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Target : MonoBehaviour
{
    private void Start()
    {
        GetComponent<VisualEffect>()?.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("AUV"))
        {
            SendMessageUpwards("TargetFound");
            tag = "InactiveTarget";
            
            GetComponent<VisualEffect>()?.Play();
            GetComponent<AudioSource>()?.Play();
        }
    }

    private void OnDrawGizmos()
    {
        UnityEngine.Gizmos.color = tag.Equals("ActiveTarget") ? Color.yellow : Color.grey;
        UnityEngine.Gizmos.DrawSphere(transform.position, .1f);
    }
}
