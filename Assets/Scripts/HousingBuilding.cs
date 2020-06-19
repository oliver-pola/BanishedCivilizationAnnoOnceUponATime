using System.Linq;
using UnityEngine;

public class HousingBuilding : Building
{
    #region Attributes
    public float childrenSpawnCount; // The number of children to spawn per economy interval
    public float immediateSpawnCount; // The number of grown workers to spawn when built
    public int immediateSpawnAge = 30; // The age of the immediate spawned workers
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
    // Is called after building was built
    protected override void EconomyInited()
    {
        // immediately spawn grown workers
        for(int i = 0; i < immediateSpawnCount; i++)
        {
            Worker w = WorkerSpawn();
            w.age = immediateSpawnAge;
            w.happiness = 1f;
            w.home = this;
            WorkerAssignedToBuilding(w);
            _jobManager.RegisterWorker(w);
        }
        StartCoroutine(EventAnim());
    }

    // Calculate efficiency based on neighbors, can be overwritten by spcecialization
    protected override void EconomyCheckEfficiency()
    {
        if (workers.Count > 0)
        {
            // efficiency is average of all workers happyness
            efficiency = workers.Sum(x => x.happiness) / workers.Count;
        }
        else
        {
            // no workers, no offspring, no efficieny
            // TODO: this building will be empty forever
            efficiency = 0f;
        }
    }

    // Additional condition for economy action (enough resources)
    protected override bool EconomyEnableAction(Warehouse warehouse)
    {
        // action is to spawn new workers, only enable when capacity available
        return workers.Count < workerCapacity;
    }

    // Economy interval is happening, do production
    protected override void EconomyAction(Warehouse warehouse)
    {
        // spawn children
        for (int i = 0; i < childrenSpawnCount; i++)
        {
            Worker w = WorkerSpawn();
            w.age = 0;
            w.happiness = 1f;
            w.transform.localScale = Vector3.one * 0.7f;
            w.home = this;
            WorkerAssignedToBuilding(w);
        }
    }
    #endregion
}
