using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Exposed to Unity Editor
    // Map generation
    public Texture2D heightmap;
    public Transform tileHolder;
    public GameObject[] waterTiles, sandTiles, grassTiles, forrestTiles, stoneTiles, mountainTiles;
    public float tileWidth;
    public float heightScaling;
    public GameObject[] hideOnStart; // contains prefabs, but must be hidden on start

    // Mouse and Camera control
    public GameObject mouseManager;
    public GameObject selectionHighlight;

    // UI, may get outsourced later
    public Text resourceText;

    // Buildings
    public GameObject[] buildingPrefabs; //References to the building prefabs

    // Economy
    public float income = 100f;
    #endregion

    #region Public for other code
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
    // The current index used for choosing a prefab to spawn from the buildingPrefabs list
    // Default is 9 (key: 0) and probably better means "no building"
    private int _selectedBuildingPrefabIndex = 9;
    // Contains the preview prefab for selected building
    private GameObject _selectedBuildingPreview;
    // Colors the building preview green if placable, red otherwise
    private MaterialPropertyBlock _selectedBuildingPreviewProperty;

    // Economy
    private Warehouse _warehouse;
    private float _economyTimer = 0f;
    #endregion

    #region Game loop
    // Start is called before the first frame update
    void Start()
    {
        _warehouse = new Warehouse();

        // Generate the map and let it set the boundaries
        GenerateMap();
        FindNeighborsOfTiles();

        // Get the height of the highlight, so you can adjust in in the editor
        if (selectionHighlight)
        {
            _selectionHighlightElevation = selectionHighlight.transform.position.y;
        }
        // Property to color building preview
        _selectedBuildingPreviewProperty = new MaterialPropertyBlock();

        // Hide prefab design containers
        foreach (var obj in hideOnStart)
        {
            obj.SetActive(false);
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
        // Register to the mouse events
        mm.OnMouseClick += GameManager_OnMouseClick;
        mm.OnMouseOver += GameManager_OnMouseOver;
        // Start the MouseManager (the GameObject)
        mouseManager.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        HandleKeyboardInput();

        _economyTimer += Time.deltaTime;
        if (_economyTimer >= 1f) // every second
        {
            _economyTimer %= 1f; // reset
            EconomyCycle();
        }

        UpdateUI();
    }
    #endregion

    #region Events
    // Handle a mouse click
    private void GameManager_OnMouseClick(GameObject source, CameraRayEventArgs e)
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

    // Handle the current mouse position
    private void GameManager_OnMouseOver(GameObject source, CameraRayEventArgs e)
    {
        // Check what the ray from camera through mouse position does collide with and highlight that

        // Select tiles, they are in layer 8
        // Also let it collide with background water (layer 4) but ignore if over water
        int layerMask = LayerMask.GetMask("Tiles") | LayerMask.GetMask("Water"); ;
        if (Physics.Raycast(e.Ray, out RaycastHit hit, Mathf.Infinity, layerMask)
            && hit.collider.gameObject.layer == LayerMask.NameToLayer("Tiles"))
        {
            Highlight(hit.collider.gameObject);
        }
        else
        {
            Highlight(null);
        }
    }
    #endregion

    #region UI Methods
    // Very basic UI
    private void UpdateUI()
    {
        resourceText.text = _warehouse.ToString();
    }

    // Sets the index for the currently selected building prefab by checking key presses on the numbers 1 to 0
    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectBuilding(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectBuilding(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectBuilding(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectBuilding(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SelectBuilding(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SelectBuilding(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SelectBuilding(6);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SelectBuilding(7);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SelectBuilding(8);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SelectBuilding(9);
        }
    }

    // Selects a game object on mouse click
    public void Select(GameObject obj)
    {
        if (obj)
        {
            Tile tile = obj.GetComponent<Tile>();
            if (tile)
            {
                TileClicked(tile);
            }
        }
    }

    // Highlights a tile
    public void Highlight(GameObject obj)
    {
        if (obj)
        {
            if (selectionHighlight)
            {
                Vector3 pos = obj.transform.position;
                pos.y += _selectionHighlightElevation;
                selectionHighlight.transform.position = pos;
                selectionHighlight.transform.rotation = obj.transform.rotation;
                selectionHighlight.SetActive(true);
            }
            // check if building placeable, give visual feedback
            if (_selectedBuildingPrefabIndex < buildingPrefabs.Length)
            {
                Tile tile = obj.GetComponent<Tile>();
                var building = buildingPrefabs[_selectedBuildingPrefabIndex].GetComponent<ProductionBuilding>();
                if (BuildingCanBeBuiltOnTile(building, tile))
                {
                    _selectedBuildingPreviewProperty.SetColor("_Color", Color.green);
                }
                else
                {
                    _selectedBuildingPreviewProperty.SetColor("_Color", Color.red);
                }
                if (_selectedBuildingPreview)
                {
                    // set material properties for all renderer components and subcomponents
                    Renderer[] renderers = _selectedBuildingPreview.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        renderer.SetPropertyBlock(_selectedBuildingPreviewProperty);
                    }
                }
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

    // Is called by MouseClick event, forwards the tile to the method for spawning buildings
    private void TileClicked(Tile tile)
    {
        string s = "";
        foreach (var t in tile.neighborTiles) s += t.type + ", ";
        Debug.Log("Clicked: " + tile.type + ", neighbors: " + s);

        PlaceBuildingOnTile(tile);
    }
    #endregion

    #region Map Methods
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

        // Store tiles for later reference
        _tileMap = new Tile[heightmap.height, heightmap.width];

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

                // Create a new GameObject
                GameObject newTileObject = Instantiate(tile, position, rotation, tileHolder);

                // Store the reference to its Tile script instance
                Tile newTile = newTileObject.GetComponent<Tile>();
                newTile.coordinateHeight = y;
                newTile.coordinateWidth = x;
                _tileMap[y, x] = newTile;
            }
        }
    }

    // Iterates through the map and stores neighbor information
    private void FindNeighborsOfTiles()
    {
        for (int h = 0; h < heightmap.height; h++)
        {
            for (int w = 0; w < heightmap.width; w++)
            {
                _tileMap[h, w].neighborTiles = FindNeighborsOfTile(_tileMap[h, w]);
            }
        }
    }

    // Returns a list of all neighbors of a given tile
    private List<Tile> FindNeighborsOfTile(Tile t)
    {
        List<Tile> result = new List<Tile>();

        int h = t.coordinateHeight;
        int w = t.coordinateWidth;

        bool hasUp = h < heightmap.height - 1;
        bool hasDown = h > 0;
        bool hasLeft = w > 0;
        bool hasRight = w < heightmap.width - 1;

        if (hasUp) result.Add(_tileMap[h + 1, w]);
        if (hasDown) result.Add(_tileMap[h - 1, w]);
        if (hasLeft) result.Add(_tileMap[h, w - 1]);
        if (hasRight) result.Add(_tileMap[h, w + 1]);
        // if h is even, [h+1, w] is top right, [h-1, w] is bottom right
        // if h is odd, [h+1, w] is top left, [h-1, w] is bottom left
        // so also add the other corner with w-1 or w+1 depending on h
        if (h % 2 == 0)
        {
            if (hasUp && hasLeft) result.Add(_tileMap[h + 1, w - 1]);
            if (hasDown && hasLeft) result.Add(_tileMap[h - 1, w - 1]);
        }
        else
        {
            if (hasUp && hasRight) result.Add(_tileMap[h + 1, w + 1]);
            if (hasDown && hasRight) result.Add(_tileMap[h - 1, w + 1]);
        }

        return result;
    }
    #endregion

    #region Economy Methods
    // Simulate economy, is called every second
    private void EconomyCycle()
    {
        // unconditional basic income
        _warehouse.AddResource(Warehouse.ResourceTypes.Money, income);

        EconomyCheckBuildings();
    }

    // Check upkeep for all buildings, produce if upkeep can be spent
    private void EconomyCheckBuildings()
    {
        // Check all tiles for buildings
        foreach (var tile in _tileMap)
        {
            if (tile.building)
            {
                tile.building.EconomyCycle(_warehouse);
            }
        }
    }
    #endregion

    #region Building Placement Methods
    // Selects the prefab to build, cares about preview
    private void SelectBuilding(int index)
    {
        if (index != _selectedBuildingPrefabIndex)
        {
            if (_selectedBuildingPreview)
            {
                Destroy(_selectedBuildingPreview);
                _selectedBuildingPreview = null;
            }
            if (index < buildingPrefabs.Length && selectionHighlight != null)
            {
                // Create a new GameObject with the building having selectionHighlight as parent
                _selectedBuildingPreview = Instantiate(buildingPrefabs[index], selectionHighlight.transform);
            }
            _selectedBuildingPrefabIndex = index;
        }
    }

    private bool BuildingCanBeBuiltOnTile(ProductionBuilding building, Tile tile)
    {
        return tile.building == null && building.canBeBuiltOnTileTypes.Contains(tile.type) &&
            _warehouse.HasResource(Warehouse.ResourceTypes.Money, building.buildCostMoney) &&
            _warehouse.HasResource(Warehouse.ResourceTypes.Planks, building.buildCostPlanks);
    }

    // Checks if the currently selected building type can be placed on the given tile and then instantiates an instance of the prefab
    private void PlaceBuildingOnTile(Tile t)
    {
        //if there is building prefab for the number input
        if (_selectedBuildingPrefabIndex < buildingPrefabs.Length)
        {
            // check if building can be placed and then istantiate it
            var prefab = buildingPrefabs[_selectedBuildingPrefabIndex].GetComponent<ProductionBuilding>();

            if (BuildingCanBeBuiltOnTile(prefab, t))
            {
                // Create a new GameObject having the tiles' GameObject as parent
                GameObject newBuildingObject = Instantiate(buildingPrefabs[_selectedBuildingPrefabIndex], t.gameObject.transform);

                // link the scripts together, cyclic :-(
                var b = newBuildingObject.GetComponent<ProductionBuilding>();
                t.building = b;
                b.tile = t;

                // hide some decoration to see the building
                t.hideOnBuilding.SetActive(false);

                // consume build costs
                _warehouse.RemoveResource(Warehouse.ResourceTypes.Money, prefab.buildCostMoney);
                _warehouse.RemoveResource(Warehouse.ResourceTypes.Planks, prefab.buildCostPlanks);
            }
            // delete buildings, for testing only
            else if (t.building != null)
            {
                Destroy(t.building.gameObject);
                t.building = null;

                // show all decoration again
                t.hideOnBuilding.SetActive(true);
            }
        }
    }
    #endregion
}
