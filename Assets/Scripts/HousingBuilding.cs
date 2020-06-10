using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingBuilding : Building
{
    #region Attributes
    public float childrenSpawnInterval; // If operating at 100% efficiency, this is the time in seconds it takes to spawn new children
    public float childrenSpawnProgress = 0f; // This is the time spent in "waiting for children" so far
    public float childrenSpawnCount; // The number of output resources per generation cycle(for example the Sawmill produces 2 planks at a time)
    #endregion

    #region Game Loop
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
    #endregion
}
