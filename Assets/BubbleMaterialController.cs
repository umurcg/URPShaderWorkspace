using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleMaterialController : MonoBehaviour
{
    public float radius=0f;
    public Material material;
    public Transform player;

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (material == null)
        {
            material = GetComponent<Renderer>().sharedMaterial;
        }
        
        if(material && player)
        {
            material.SetFloat("_Radius", radius);
            material.SetVector("_Center", player.transform.position);
        }
    }

    private void OnValidate()
    {
        Refresh();
    }
}
