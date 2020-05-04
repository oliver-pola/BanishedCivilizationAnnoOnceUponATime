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
}
