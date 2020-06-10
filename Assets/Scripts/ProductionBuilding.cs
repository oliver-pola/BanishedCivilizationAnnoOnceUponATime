using System;
using System.Collections.Generic;
using UnityEngine;

public class ProductionBuilding : MonoBehaviour
{
    #region Attributes
    public BuildingTypes type; // The name of the building
    public float upkeep; // The money cost per minute
    public float buildCostMoney; // placement money cost
    public float buildCostPlanks; // placement planks cost
    public Tile tile; // Reference to the tile it is built on (oh no, not cyclic references again!)
    public float efficiency = 1f; // Calculated based on the surrounding tile types
    public float resourceGenerationInterval; // If operating at 100% efficiency, this is the time in seconds it takes for one production cycle to finish
    public float resourceGenerationProgress = 0f; // This is the time spent in production so far
    public float outputCount; // The number of output resources per generation cycle(for example the Sawmill produces 2 planks at a time)
    public List<Tile.TileTypes> canBeBuiltOnTileTypes; // A restriction on which types of tiles it can be placed on
    public Tile.TileTypes efficiencyScalesWithNeighboringTiles = Tile.TileTypes.Empty; // A choice if its efficiency scales with a specific type of surrounding tile
    public int minimumNeighbors; // The minimum number of surrounding tiles its efficiency scales with(0-6)
    public int maximumNeighbors; // The maximum number of surrounding tiles its efficiency scales with(0-6)
    public List<GameManager.ResourceTypes> inputResources = new List<GameManager.ResourceTypes>(); // A choice for input resource types(0, 1 or 2 types)
    public GameManager.ResourceTypes outputResource; // A choice for output resource type
    public GameObject resourceAnim; // Gets activeted when resources produced
    #endregion

    #region Enumerations
    public enum BuildingTypes { Fishery, Lumberjack, Sawmill, SheepFarm, FrameworkKnitters, PotatoFarm, SchnappsDistillery };
    #endregion

    #region Manager References
    JobManager _jobManager; //Reference to the JobManager
    #endregion

    #region Workers
    public List<Worker> _workers; //List of all workers associated with this building, either for work or living
    #endregion

    #region Jobs
    public List<Job> _jobs; // List of all available Jobs. Is populated in Start()
    #endregion

    #region Methods   
    public void WorkerAssignedToBuilding(Worker w)
    {
        _workers.Add(w);
    }

    public void WorkerRemovedFromBuilding(Worker w)
    {
        _workers.Remove(w);
    }
    #endregion
}
