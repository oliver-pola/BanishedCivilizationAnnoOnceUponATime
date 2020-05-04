using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float SceneMaxX { get; private set; }
    public float SceneMinX { get; private set; }
    public float SceneMaxZ { get; private set; }
    public float SceneMinZ { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        SceneMaxX = 100f;
        SceneMinX = -100f;
        SceneMaxZ = 100f;
        SceneMinZ = -100f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select(Ray ray)
    {
        // Select tiles, they are in layer 8
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Select(hit.collider.gameObject);
        }
    }

    public void Select(GameObject obj)
    {
        Debug.Log("Clicked on " + obj.name + " at " + obj.transform.position);
    }
}
