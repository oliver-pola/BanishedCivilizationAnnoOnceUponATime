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
    public Texture2D heightmap;
    public GameObject[] waterTiles, sandTiles, grassTiles, forrestTiles, stoneTiles, mountainTiles;
    public GameObject mouseManager;
    public GameObject selectionHighlight;
    public float tileWidth;
    public float heightScaling;
    public float SceneMaxX { get; private set; }
    public float SceneMinX { get; private set; }
    public float SceneMaxZ { get; private set; }
    public float SceneMinZ { get; private set; }
    public float SceneStartX { get; private set; }
    public float SceneStartZ { get; private set; }

    private float selectionHighlightElevation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        SceneMaxX = heightmap.width * tileWidth;
        SceneMinX = 0;
        SceneMaxZ = heightmap.height * tileWidth * (float) Math.Sin(Math.PI /3);
        SceneMinZ = 0;
        SceneStartX = (SceneMaxX - SceneMinX) / 2.0f;
        SceneStartZ = (SceneMaxZ - SceneMinZ) / 2.0f;

        // Get the height of the highlight, so you can adjust in in the editor
        if (selectionHighlight)
        {
            selectionHighlightElevation = selectionHighlight.transform.position.y;
        }

        GenerateMap();
        mouseManager.SetActive(true);
        // If we want to push the camera position instead of let the MouseManager pull it from here:
        // mouseManager.GetComponent<MouseManager>().SetCameraGroundPosition(SceneStartX, SceneStartZ);
    }

    private void GenerateMap()
    {
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
                Instantiate(tile, position, rotation);
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

    public void Select(Ray ray)
    {
        // Select tiles, they are in layer 8
        // Also let it collide with background water (layer 4) but ignore if water was clicked
        int layerMask = 1 << 8 | 1 << 4;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)
            && hit.collider.gameObject.layer == 8)
        {
            Select(hit.collider.gameObject);
        }
        else
        {
            if (selectionHighlight)
            {
                selectionHighlight.SetActive(false);
            }
        }
    }

    public void Select(GameObject obj)
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
}
