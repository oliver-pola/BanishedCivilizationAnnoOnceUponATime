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
    // Exposed to Unity Editor
    public Texture2D heightmap;
    public Transform tileHolder;
    public GameObject[] waterTiles, sandTiles, grassTiles, forrestTiles, stoneTiles, mountainTiles;
    public GameObject mouseManager;
    public GameObject selectionHighlight;
    public float tileWidth;
    public float heightScaling;

    // Public for other code
    public float SceneMaxX { get; private set; }
    public float SceneMinX { get; private set; }
    public float SceneMaxZ { get; private set; }
    public float SceneMinZ { get; private set; }

    // Private fields
    private float selectionHighlightElevation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // Generate the map and let it set the boundaries
        GenerateMap();

        // Get the height of the highlight, so you can adjust in in the editor
        if (selectionHighlight)
        {
            selectionHighlightElevation = selectionHighlight.transform.position.y;
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

        for(int x = 0; x < heightmap.width; x++)
        {
            for(int y = 0; y < heightmap.height; y++)
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
                position.z = y * tileWidth * Mathf.Sin(Mathf.PI/3); // radians, because c# is SOMETIMES a reasonable language
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

    // Update is called once per frame
    void Update()
    {

    }

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

    // Selects a game object
    public void Select(GameObject obj)
    {
        if (obj)
        {
            Debug.Log("Clicked on " + obj.name + " at " + obj.transform.position);
            if (selectionHighlight)
            {
                Vector3 pos = obj.transform.position;
                pos.y += selectionHighlightElevation;
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
}
