using System.Collections.Generic;
using System.Linq;

public class ProductionBuilding : Building
{
    #region Attributes
    public List<Warehouse.ResourceTypes> inputResources = new List<Warehouse.ResourceTypes>(); // A choice for input resource types(0, 1 or 2 types)
    public Warehouse.ResourceTypes outputResource; // A choice for output resource type
    public float outputCount; // The number of output resources per economy interval
    #endregion

    #region Game Loop
    // Start is called before the first frame update, TODO remove if not needed
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame, TODO remove if not needed
    protected override void Update()
    {
        base.Update();
    }
    #endregion

    #region Economy Methods
    // Additional condition for economy action (enough resources)
    protected override bool EconomyEnableAction(Warehouse warehouse)
    {
        return inputResources.All(x => warehouse.HasResource(x));
    }

    // Economy interval is happening, do production
    protected override void EconomyAction(Warehouse warehouse)
    {
        // consume
        foreach (var res in inputResources)
            warehouse.RemoveResource(res, 1);
        // produce
        warehouse.AddResource(outputResource, outputCount);
    }
    #endregion
}
