using UnityEngine;

public class HousingBuilding : Building
{
    #region Attributes
    public float childrenSpawnCount; // The number of children to spawn per economy interval
    public float immediateSpawnCount; // The number of grown workers to spawn when built
    public float immediateSpawnAge = 30f; // The age of the immediate spawned workers
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
            WorkerAssignedToBuilding(w);
        }
        StartCoroutine(EventAnim());
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
            w.age = 0f;
            w.happiness = 1f;
            w.transform.localScale = Vector3.one * 0.7f;
            WorkerAssignedToBuilding(w);
        }
    }
    #endregion
}
