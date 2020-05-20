using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class GameManager : MonoBehaviour
{
    #region Exposed to Unity Editor
    // Map generation
    public Texture2D heightmap;
    public Transform tileHolder;
    public GameObject[] waterTiles, sandTiles, grassTiles, forrestTiles, stoneTiles, mountainTiles;
    public float tileWidth;
    public float heightScaling;

    // Mouse and Camera control
    public GameObject mouseManager;
    public GameObject selectionHighlight;

    // Buildings
    public GameObject[] buildingPrefabs; //References to the building prefabs
    #endregion

    #region Public for other code
    // Enumerations
    public enum ResourceTypes { None, Fish, Wood, Planks, Wool, Clothes, Potato, Schnapps }; //Enumeration of all available resource types. Can be addressed from other scripts by calling GameManager.ResourceTypes

    // Map boundaries
    public float SceneMaxX { get; private set; }
    public float SceneMinX { get; private set; }
    public float SceneMaxZ { get; private set; }
    public float SceneMinZ { get; private set; }
    #endregion

    #region Private fields
    // Map generation
    private Tile[,] _tileMap; //2D array of all spawned tiles

    // Mouse and Camera control
    private float _selectionHighlightElevation = 0f;

    // Buildings
    private int _selectedBuildingPrefabIndex = 0; //The current index used for choosing a prefab to spawn from the buildingPrefabs list

    // Resources
    private Dictionary<ResourceTypes, float> _resourcesInWarehouse = new Dictionary<ResourceTypes, float>(); //Holds a number of stored resources for every ResourceType

    //A representation of _resourcesInWarehouse, broken into individual floats. Only for display in inspector, will be removed and replaced with UI later
    [SerializeField]
    private float _ResourcesInWarehouse_Fish;
    [SerializeField]
    private float _ResourcesInWarehouse_Wood;
    [SerializeField]
    private float _ResourcesInWarehouse_Planks;
    [SerializeField]
    private float _ResourcesInWarehouse_Wool;
    [SerializeField]
    private float _ResourcesInWarehouse_Clothes;
    [SerializeField]
    private float _ResourcesInWarehouse_Potato;
    [SerializeField]
    private float _ResourcesInWarehouse_Schnapps;
    #endregion

    #region Game loop
    // Start is called before the first frame update
    void Start()
    {
        PopulateResourceDictionary();

        // Generate the map and let it set the boundaries
        GenerateMap();

        // Get the height of the highlight, so you can adjust in in the editor
        if (selectionHighlight)
        {
            _selectionHighlightElevation = selectionHighlight.transform.position.y;
        }

        // Start camera in the center
        float SceneStartX = (SceneMaxX - SceneMinX) / 2.0f;
        float SceneStartZ = (SceneMaxZ - SceneMinZ) / 2.0f;

        // Setup the MouseManager (the Script attached to the GameObject)
        MouseManager mm = mouseManager.GetComponent<MouseManager>();
        mm.SceneMaxX = SceneMaxX;
        mm.SceneMinX = SceneMinX;
        mm.SceneMaxZ = SceneMaxZ;
        mm.SceneMinX = SceneMinZ;
        mm.SetCameraGroundPosition(SceneStartX, SceneStartZ);
        // Register to the mouse click event
        mm.OnMouseClick += GameManager_OnMouseClick;
        // Start the MouseManager (the GameObject)
        mouseManager.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        HandleKeyboardInput();
        UpdateInspectorNumbersForResources();
    }
    #endregion

    #region Events
    // Handle a mouse click
    private void GameManager_OnMouseClick(GameObject source, MouseClickEventArgs e)
    {
        // Check what the ray from camera through mouse position does collide with and select that

        // Select tiles, they are in layer 8
        // Also let it collide with background water (layer 4) but ignore if water was clicked
        int layerMask = LayerMask.GetMask("Tiles") | LayerMask.GetMask("Water"); ;
        if (Physics.Raycast(e.Ray, out RaycastHit hit, Mathf.Infinity, layerMask)
            && hit.collider.gameObject.layer == LayerMask.NameToLayer("Tiles"))
        {
            Select(hit.collider.gameObject);
        }
        else
        {
            Select(null);
        }
    }
    #endregion

    #region Methods
    // Generates a map from tiles
    private void GenerateMap()
    {
        // Set the boundaries that will be used
        SceneMaxX = (heightmap.width - 0.5f) * tileWidth;
        SceneMinX = 0;
        SceneMaxZ = (heightmap.height - 1) * tileWidth * (float)Math.Sin(Math.PI / 3);
        SceneMinZ = 0;

        // Some deterministic randomness for tile variations
        System.Random rand = new System.Random(0);

        for (int x = 0; x < heightmap.width; x++)
        {
            for (int y = 0; y < heightmap.height; y++)
            {
                Color pixel = heightmap.GetPixel(x, y);
                float height = Math.Max(Math.Max(pixel.r, pixel.g), pixel.b);
                GameObject tile;
                if (height == 0)
                {
                    tile = waterTiles[rand.Next(waterTiles.Count())];
                }
                else if (height > 0.0 && height <= 0.2)
                {
                    tile = sandTiles[rand.Next(sandTiles.Count())]; ;
                }
                else if (height > 0.2 && height <= 0.4)
                {
                    tile = grassTiles[rand.Next(grassTiles.Count())]; ;
                }
                else if (height > 0.4 && height <= 0.6)
                {
                    tile = forrestTiles[rand.Next(forrestTiles.Count())]; ;
                }
                else if (height > 0.6 && height <= 0.8)
                {
                    tile = stoneTiles[rand.Next(stoneTiles.Count())]; ;
                }
                else if (height > 0.8 && height <= 1.0)
                {
                    tile = mountainTiles[rand.Next(mountainTiles.Count())]; ;
                }
                else
                {
                    Debug.LogError("Incorrect Height Data in Heightmap, defaulting to Water Tile!");
                    tile = waterTiles[rand.Next(waterTiles.Count())]; ;
                }
                Vector3 position = new Vector3();
                position.x = x * tileWidth + y % 2 * 0.5f * tileWidth;
                position.y = height * heightScaling;
                position.z = y * tileWidth * Mathf.Sin(Mathf.PI / 3); // radians, because c# is SOMETIMES a reasonable language
                Quaternion rotation = new Quaternion();
                int rotY = 30 + rand.Next(6) * 60; // some variation of the tiles by simple rotation
                rotation.eulerAngles = new Vector3(0, rotY, 0); // why the fuck does unity use degrees?
                Instantiate(tile, position, rotation, tileHolder);
            }
        }
        // set original tiles inactive so they dont show
        foreach (var tile in waterTiles)
            tile.SetActive(false);
        foreach (var tile in sandTiles)
            tile.SetActive(false);
        foreach (var tile in grassTiles)
            tile.SetActive(false);
        foreach (var tile in forrestTiles)
            tile.SetActive(false);
        foreach (var tile in stoneTiles)
            tile.SetActive(false);
        foreach (var tile in mountainTiles)
            tile.SetActive(false);
    }

    // Selects a game object
    public void Select(GameObject obj)
    {
        if (obj)
        {
            Debug.Log("Clicked on " + obj.name + " at " + obj.transform.position);
            if (selectionHighlight)
            {
                Vector3 pos = obj.transform.position;
                pos.y += _selectionHighlightElevation;
                selectionHighlight.transform.position = pos;
                selectionHighlight.SetActive(true);
            }
        }
        else
        {
            if (selectionHighlight)
            {
                selectionHighlight.SetActive(false);
            }
        }
    }
    void PopulateResourceDictionary()
    {
        _resourcesInWarehouse.Add(ResourceTypes.None, 0);
        _resourcesInWarehouse.Add(ResourceTypes.Fish, 0);
        _resourcesInWarehouse.Add(ResourceTypes.Wood, 0);
        _resourcesInWarehouse.Add(ResourceTypes.Planks, 0);
        _resourcesInWarehouse.Add(ResourceTypes.Wool, 0);
        _resourcesInWarehouse.Add(ResourceTypes.Clothes, 0);
        _resourcesInWarehouse.Add(ResourceTypes.Potato, 0);
        _resourcesInWarehouse.Add(ResourceTypes.Schnapps, 0);
    }

    //Sets the index for the currently selected building prefab by checking key presses on the numbers 1 to 0
    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _selectedBuildingPrefabIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _selectedBuildingPrefabIndex = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _selectedBuildingPrefabIndex = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _selectedBuildingPrefabIndex = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _selectedBuildingPrefabIndex = 4;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            _selectedBuildingPrefabIndex = 5;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            _selectedBuildingPrefabIndex = 6;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            _selectedBuildingPrefabIndex = 7;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            _selectedBuildingPrefabIndex = 8;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _selectedBuildingPrefabIndex = 9;
        }
    }

    //Updates the visual representation of the resource dictionary in the inspector. Only for debugging
    void UpdateInspectorNumbersForResources()
    {
        _ResourcesInWarehouse_Fish = _resourcesInWarehouse[ResourceTypes.Fish];
        _ResourcesInWarehouse_Wood = _resourcesInWarehouse[ResourceTypes.Wood];
        _ResourcesInWarehouse_Planks = _resourcesInWarehouse[ResourceTypes.Planks];
        _ResourcesInWarehouse_Wool = _resourcesInWarehouse[ResourceTypes.Wool];
        _ResourcesInWarehouse_Clothes = _resourcesInWarehouse[ResourceTypes.Clothes];
        _ResourcesInWarehouse_Potato = _resourcesInWarehouse[ResourceTypes.Potato];
        _ResourcesInWarehouse_Schnapps = _resourcesInWarehouse[ResourceTypes.Schnapps];
    }

    //Checks if there is at least one material for the queried resource type in the warehouse
    public bool HasResourceInWarehoues(ResourceTypes resource)
    {
        return _resourcesInWarehouse[resource] >= 1;
    }

    //Is called by MouseManager when a tile was clicked
    //Forwards the tile to the method for spawning buildings
    public void TileClicked(int height, int width)
    {
        Tile t = _tileMap[height, width];

        PlaceBuildingOnTile(t);
    }

    //Checks if the currently selected building type can be placed on the given tile and then instantiates an instance of the prefab
    private void PlaceBuildingOnTile(Tile t)
    {
        //if there is building prefab for the number input
        if (_selectedBuildingPrefabIndex < buildingPrefabs.Length)
        {
            //TODO: check if building can be placed and then istantiate it

        }
    }

    //Returns a list of all neighbors of a given tile
    private List<Tile> FindNeighborsOfTile(Tile t)
    {
        List<Tile> result = new List<Tile>();

        //TODO: put all neighbors in the result list

        return result;
    }
    #endregion
}
