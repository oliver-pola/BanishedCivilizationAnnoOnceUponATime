using System.Collections.Generic;
using System.Linq;

public class ProductionBuilding : Building
{
    #region Attributes
    public List<Warehouse.ResourceTypes> inputResources = new List<Warehouse.ResourceTypes>(); // A choice for input resource types(0, 1 or 2 types)
    public Warehouse.ResourceTypes outputResource; // A choice for output resource type
    public float outputCount; // The number of output resources per economy interval
    public List<Job> jobs; // List of all available Jobs. Is populated when built.
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

    protected override void OnDestroy()
    {
        // Remove jobs from workers
        if (_jobManager != null && jobs != null)
            _jobManager.RemoveJob(jobs);
    }
    #endregion

    #region Economy Methods
    // Called when building was built, register jobs
    protected override void EconomyInited()
    {
        jobs = new List<Job>(workerCapacity);
        for (int i = 0; i < workerCapacity; i++)
        {
            Job job = new Job(this);
            jobs.Add(job);
            _jobManager.RegisterJob(job);
        }
    }

    // Calculate efficiency based on workers assigned and their happiness
    protected override void EconomyCheckEfficiency()
    {
        base.EconomyCheckEfficiency();

        if (workers.Count > 0)
        {
            // efficiency is average of all workers happiness
            // using capacity for total amount does count unassigned jobs with 0 happiness
            efficiency *= workers.Sum(x => x.happiness) / workerCapacity;
        }
        else
        {
            // no workers, no production
            efficiency = 0f;
        }
    }

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
