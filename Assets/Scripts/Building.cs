using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Building : MonoBehaviour
{
    #region Attributes
    public BuildingTypes type; // The name of the building
    public float upkeep; // The money cost per minute
    public float buildCostMoney; // placement money cost
    public float buildCostPlanks; // placement planks cost
    public List<Tile.TileTypes> canBeBuiltOnTileTypes; // A restriction on which types of tiles it can be placed on
    public Tile.TileTypes efficiencyScalesWithNeighboringTiles = Tile.TileTypes.Empty; // A choice if its efficiency scales with a specific type of surrounding tile
    public Tile tile; // Reference to the tile it is built on (oh no, not cyclic references again!)
    public float efficiency = 1f; // Calculated based on the surrounding tile types
    public float economyInterval; // If operating at 100% efficiency, this is the time in seconds it takes for one production / spawn cycle to finish
    public float economyProgress = 0f; // This is the time spent in production / "waiting for children" so far
    public int minimumNeighbors; // The minimum number of surrounding tiles its efficiency scales with(0-6)
    public int maximumNeighbors; // The maximum number of surrounding tiles its efficiency scales with(0-6)
    public GameObject eventAnim; // Gets activated when an event occurs, like resources produced or children spawned
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

    #region Economy Methods
    // Simulate economy, is called every second by GameManager
    public void EconomyCycle(Warehouse warehouse)
    {
        tile.building.eventAnim.SetActive(false);

        if (warehouse.HasResource(Warehouse.ResourceTypes.Money, upkeep))
        {
            warehouse.RemoveResource(Warehouse.ResourceTypes.Money, upkeep);
            EconomyCheckEfficiency();
            EconomyCheckInterval(warehouse);
        }
    }

    // Calculate efficiency based on neighbors, can be overwritten by spcecialization
    protected virtual void EconomyCheckEfficiency()
    {
        if (efficiencyScalesWithNeighboringTiles != Tile.TileTypes.Empty)
        {
            int count = tile.neighborTiles.Count(x =>
                x.type == efficiencyScalesWithNeighboringTiles &&
                x.building == null);
            if (count < minimumNeighbors)
            {
                efficiency = 0f;
            }
            else if (count >= maximumNeighbors)
            {
                efficiency = 1f;
            }
            else
            {
                efficiency = (float)count / maximumNeighbors;
            }
        }
    }

    // Additional condition for economy action (enough resources)
    protected virtual bool EconomyEnableAction(Warehouse warehouse)
    {
        return true;
    }

    // Check for time spent in economy intervall and call action
    private void EconomyCheckInterval(Warehouse warehouse)
    {
        // check progress, division by zero can happen, but is not a problem here, infinity is fine
        float productionEvery = economyInterval / efficiency;
        economyProgress += 1f; // advance one cylce = 1 second

        bool hasProgress = economyProgress >= productionEvery;

        if (hasProgress && EconomyEnableAction(warehouse))
        {
            economyProgress = 0f; // reset
            EconomyAction(warehouse);
            eventAnim.SetActive(true);
        }
    }

    // Economy interval is happening, do your work, overwritten on specialization
    protected virtual void EconomyAction(Warehouse warehouse)
    {

    }
    #endregion

    #region Worker Methods
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
