using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class GameManager : MonoBehaviour
{
    public Texture2D heightmap;
    public GameObject waterTile, sandTile, grassTile, forrestTile, stoneTile, mountainTile;
    public GameObject mouseManager;
    public float tileWidth;
    public float heightScaling;
    public float SceneMaxX { get; private set; }
    public float SceneMinX { get; private set; }
    public float SceneMaxZ { get; private set; }
    public float SceneMinZ { get; private set; }
    public float SceneStartX { get; private set; }
    public float SceneStartZ { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        SceneMaxX = heightmap.width * tileWidth;
        SceneMinX = 0;
        SceneMaxZ = heightmap.height * tileWidth * (float) Math.Sin(Math.PI /3);
        SceneMinZ = 0;
        SceneStartX = (SceneMaxX - SceneMinX) / 2.0f;
        SceneStartZ = (SceneMaxZ - SceneMinZ) / 2.0f;

        GenerateMap();
        mouseManager.SetActive(true);
    }

    private void GenerateMap()
    {
        for(int x = 0; x < heightmap.width; x++)
        {
            for(int y = 0; y < heightmap.height; y++)
            {
                Color pixel = heightmap.GetPixel(x, y);
                float height = Math.Max(Math.Max(pixel.r, pixel.g), pixel.b);
                GameObject tile;
                if (height == 0)
                {
                    tile = waterTile;
                }
                else if (height > 0.0 && height <= 0.2)
                {
                    tile = sandTile;
                }
                else if (height > 0.2 && height <= 0.4)
                {
                    tile = grassTile;
                }
                else if (height > 0.4 && height <= 0.6)
                {
                    tile = forrestTile;
                }
                else if (height > 0.6 && height <= 0.8)
                {
                    tile = stoneTile;
                }
                else if (height > 0.8 && height <= 1.0)
                {
                    tile = mountainTile;
                }
                else
                {
                    Debug.LogError("Incorrect Height Data in Heightmap, defaulting to Water Tile!");
                    tile = waterTile;
                }
                Vector3 position = new Vector3();
                position.x = x * tileWidth + y % 2 * 0.5f * tileWidth;
                position.y = height * heightScaling; 
                position.z = y * tileWidth * Mathf.Sin(Mathf.PI/3); // radians, because c# is SOMETIMES a reasonable language
                Quaternion rotation = new Quaternion();
                rotation.eulerAngles = new Vector3(0, 90, 0); // why the fuck does unity use degrees?
                Instantiate(tile, position, rotation);
            }
        }
        // set original tiles inactive so they dont show
        waterTile.SetActive(false);
        sandTile.SetActive(false);
        grassTile.SetActive(false);
        forrestTile.SetActive(false);
        stoneTile.SetActive(false);
        mountainTile.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select(Ray ray)
    {
        // Select tiles, they are in layer 8
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Select(hit.collider.gameObject);
        }
    }

    public void Select(GameObject obj)
    {
        Debug.Log("Clicked on " + obj.name + " at " + obj.transform.position);
    }
}
