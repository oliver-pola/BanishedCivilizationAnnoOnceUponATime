using System;
using System.Collections.Generic;
using UnityEngine;

public class ProductionBuilding : Building
{
    #region Attributes
    public float resourceGenerationInterval; // If operating at 100% efficiency, this is the time in seconds it takes for one production cycle to finish
    public float resourceGenerationProgress = 0f; // This is the time spent in production so far
    public float outputCount; // The number of output resources per generation cycle(for example the Sawmill produces 2 planks at a time)
    public List<Warehouse.ResourceTypes> inputResources = new List<Warehouse.ResourceTypes>(); // A choice for input resource types(0, 1 or 2 types)
    public Warehouse.ResourceTypes outputResource; // A choice for output resource type
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
