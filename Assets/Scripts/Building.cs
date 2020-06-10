using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    #region Attributes
    public BuildingTypes type; // The name of the building
    public float upkeep; // The money cost per minute
    public float buildCostMoney; // placement money cost
    public float buildCostPlanks; // placement planks cost
    public Tile tile; // Reference to the tile it is built on (oh no, not cyclic references again!)
    public float efficiency = 1f; // Calculated based on the surrounding tile types
    public List<Tile.TileTypes> canBeBuiltOnTileTypes; // A restriction on which types of tiles it can be placed on
    public Tile.TileTypes efficiencyScalesWithNeighboringTiles = Tile.TileTypes.Empty; // A choice if its efficiency scales with a specific type of surrounding tile
    public int minimumNeighbors; // The minimum number of surrounding tiles its efficiency scales with(0-6)
    public int maximumNeighbors; // The maximum number of surrounding tiles its efficiency scales with(0-6)
    public GameObject resourceAnim; // Gets activeted when resources produced
    public int workerCapacity; // jobs offered for production or living space for housing
    #endregion

    #region Enumerations
    public enum BuildingTypes { Fishery, Lumberjack, Sawmill, SheepFarm, FrameworkKnitters, PotatoFarm, SchnappsDistillery, FarmersResidence };
    #endregion

    #region Manager References
    JobManager _jobManager; //Reference to the JobManager
    #endregion

    #region Workers
    public List<Worker> workers; // List of all workers associated with this building, either for work or living
    #endregion

    #region Jobs
    public List<Job> jobs; // List of all available Jobs. Is populated in Start()
    #endregion

    #region Game Loop
    // Start is called before the first frame update
    protected virtual void Start()
    {
        jobs = new List<Job>(workerCapacity);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }
    #endregion

    #region Methods   
    public void WorkerAssignedToBuilding(Worker w)
    {
        workers.Add(w);
    }

    public void WorkerRemovedFromBuilding(Worker w)
    {
        workers.Remove(w);
    }
    #endregion
}
