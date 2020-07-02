using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

public class Building : MonoBehaviour
{
    #region Attributes
    public BuildingTypes type; // The name of the building
    public float upkeep; // The money cost per minute
    public float upkeepInterval = 60f; // Upkeep is due every number of seconds
    public float buildCostMoney; // placement money cost
    public float buildCostPlanks; // placement planks cost
    public List<Tile.TileTypes> canBeBuiltOnTileTypes; // A restriction on which types of tiles it can be placed on
    public Tile.TileTypes efficiencyScalesWithNeighboringTiles = Tile.TileTypes.Empty; // A choice if its efficiency scales with a specific type of surrounding tile
    public Tile tile; // Reference to the tile it is built on (oh no, not cyclic references again!)
    public float efficiency = 1f; // Calculated based on the surrounding tile types
    public float economyInterval; // If operating at 100% efficiency, this is the time in seconds it takes for one production / spawn cycle to finish
    public float economyProgress = 0f; // This is the time spent in production / "waiting for children" so far
    [Range(0, 6)]
    public int minimumNeighbors; // The minimum number of surrounding tiles its efficiency scales with(0-6)
    [Range(0, 6)]
    public int maximumNeighbors; // The maximum number of surrounding tiles its efficiency scales with(0-6)
    public GameObject eventAnim; // Gets activated when an event occurs, like resources produced or children spawned
    public int workerCapacity; // jobs offered for production or living space for housing
    public float workerSpawnRadius = 4f; // Workers are randomly spawned on a circle around the building
    public int[,] potentialField;
    #endregion

    #region Enumerations
    public enum BuildingTypes { Fishery, Lumberjack, Sawmill, SheepFarm, FrameworkKnitters, PotatoFarm, SchnappsDistillery, FarmersResidence };
    #endregion

    #region Manager References
    protected JobManager _jobManager; // Reference to the JobManager, assigned in EconomyInit()
    protected WorkerPool _workerPool; // Reference to WorkerPool, assigned in EconomyInit()
    #endregion

    #region Workers
    public List<Worker> workers; // List of all workers associated with this building, either for work or living
    #endregion

    #region Game Loop
    // Start is called before the first frame update
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    protected virtual void OnDestroy()
    {
        // Defaults to despawn
        if (_jobManager != null && workers != null)
            _jobManager.RemoveWorker(workers);
        if (_workerPool != null && workers != null)
            _workerPool.Release(workers);
    }
    #endregion

    #region Economy Methods
    // Has to be called (by GameManager) directly after building was built
    // don't use start here, should not be called for building preview and has nothing to do with instanciation in general
    public void EconomyInit(JobManager jobManager, WorkerPool workerPool)
    {
        _jobManager = jobManager;
        _workerPool = workerPool;

        workers = new List<Worker>(workerCapacity);

        // call virtual method of specializations
        EconomyInited();
    }

    // Called when building was built, specializations can override and react
    protected virtual void EconomyInited()
    {

    }

    // Simulate economy, is called every second by GameManager
    public virtual void EconomyCycle(Warehouse warehouse)
    {
        // The economy cycle is every second and the idea of upkeep enables progress is nice, 
        // so just recalculate upkeep to a per second value
        if (warehouse.TryRemoveResource(Warehouse.ResourceTypes.Money, upkeep / upkeepInterval))
        {
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
        else
        {
            efficiency = 1f;
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
            StartCoroutine(EventAnim());
        }
        else if (hasProgress)
        {
            // don't let progress grow to infinity
            economyProgress = productionEvery;
        }
    }

    // Coroutine to show the event animation
    protected IEnumerator EventAnim()
    {
        eventAnim.SetActive(true);
        yield return new WaitForSeconds(1f);
        eventAnim.SetActive(false);
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

    public Vector3 GetWorkerSpawnPosition()
    {
        return transform.position + Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f) * Vector3.forward * workerSpawnRadius;
    }

    protected Worker WorkerSpawn()
    {
        // spawn worker unit somewhere on a circle around this building is on
        Vector3 position = transform.position;
        // rotation to look
        Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
        Worker w = _workerPool.Require(position, rotation, _jobManager);
        // move to spawn point on cirlce around building
        w.MoveTo(GetWorkerSpawnPosition());
        return w;
    }
    #endregion


    #region Potential Field
    public void UpdatePotentialField(Tile[,] tileMap, Vector2Int buildingPosition)
    {
        potentialField = new int[tileMap.GetLength(0), tileMap.GetLength(1)];
        for(int i = 0; i < potentialField.GetLength(0); i++)
        {
            for(int j = 0; j < potentialField.GetLength(1); j++)
            {
                potentialField[i, j] = int.MaxValue;
            }
        }
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(buildingPosition);
        potentialField[buildingPosition.x, buildingPosition.y] = 0;

        while (queue.Count > 0)
        {
            Vector2Int currentTile = queue.Dequeue();
            Vector2Int[] neighbours = GetNeighbours(tileMap, currentTile);

            foreach (Vector2Int neighbour in neighbours)
            {
                // get new cost
                int tile_cost = potentialField[currentTile.x, currentTile.y] + GetTileCost(tileMap[neighbour.x, neighbour.y]);
                // check if path is shorter than previously found path
                if (tile_cost < potentialField[neighbour.x, neighbour.y])
                {
                    // update cost
                    potentialField[neighbour.x, neighbour.y] = tile_cost;
                    // enqueue if not in list
                    if (!queue.Contains(neighbour))
                    {
                        queue.Enqueue(neighbour);
                    }
                }
            }
        }

        /*
        // viz neigghbours
        foreach (Vector2Int n in GetNeighbours(tileMap, buildingPosition))
        {
            Vector3 position = new Vector3();
            position.x = n.x * 10f + n.y % 2 * 0.5f * 10f;
            position.y = 15;
            position.z = n.y * 10f * Mathf.Sin(Mathf.PI / 3); // radians, because c# is SOMETIMES a reasonable language
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.transform.position = position;
            g.transform.localScale = g.transform.localScale * 6;
            Material m = new Material(Shader.Find("Standard"));
            float c = 1 - potentialField[n.x, n.y] / 20f;
            m.color = new Color(c, 0, 0, 1);
            g.GetComponent<Renderer>().material = m;
        }
        */
        /*
        // viz potfield
        for (int x = 0; x < potentialField.GetLength(0); x++)
        {
            for (int y = 0; y < potentialField.GetLength(1); y++)
            {
                Vector3 position = new Vector3();
                position.x = x * 10f + y % 2 * 0.5f * 10f;
                position.y = 15;
                position.z = y * 10f * Mathf.Sin(Mathf.PI / 3); // radians, because c# is SOMETIMES a reasonable language
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.transform.position = position;
                g.transform.localScale = g.transform.localScale * 6;
                Material m = new Material(Shader.Find("Standard"));
                float c = 1 - potentialField[x, y] / 20f;
                m.color = new Color(c, 0, 0, 1) ;
                g.GetComponent<Renderer>().material = m;
            }
            Debug.Log(debug);
            debug = "";
        }*/
    }
    private Vector2Int[] GetNeighbours(Tile[,] tileMap, Vector2Int buildingPosition)
    {
        int[] offsetX = { 0, 1, -1, 1, 0, 1 };
        int[] offsetY = {-1, -1,  0, 0, 1, 1};
        Vector2Int[] neighbours = new Vector2Int[6];
        for (int i = 0; i < 6; i++)
        {
            if (buildingPosition.x + offsetX[i] >= 0 && buildingPosition.x + offsetX[i] < tileMap.GetLength(0)
                && buildingPosition.y + offsetY[i] >= 0 && buildingPosition.y + offsetY[i] < tileMap.GetLength(1))
            {
                neighbours[i] = new Vector2Int(buildingPosition.x + offsetX[i], buildingPosition.y + offsetY[i]);
            }
        }
        return neighbours;
    }
    private int GetTileCost(Tile t)
    {
        switch (t.type)
        {
            case Tile.TileTypes.Water:
                return 30;
            case Tile.TileTypes.Sand:
                return 2;
            case Tile.TileTypes.Grass:
                return 1;
            case Tile.TileTypes.Forest:
                return 2;
            case Tile.TileTypes.Stone:
                return 1;
            case Tile.TileTypes.Mountain:
                return 3;
            default:
                throw new System.ArgumentException("Invalid tyle type in getValueOfTile method, cost could not be calculated");
        }
    }
    #endregion
}
